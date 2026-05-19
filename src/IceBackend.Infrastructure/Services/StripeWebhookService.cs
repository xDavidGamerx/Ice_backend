using System.Text.Json;
using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IceBackend.Infrastructure.Services
{
    /// <summary>
    /// Caso de uso de procesamiento de webhooks. Implementa la doble barrera de idempotencia:
    ///   1ª Barrera: Redis — rechazo rápido en memoria (O(1)) con TTL de 24h.
    ///   2ª Barrera: PostgreSQL — unicidad en DB garantizada por índice único en ProviderEventId.
    /// </summary>
    public class StripeWebhookService : IStripeWebhookService
    {
        // Tipos de evento soportados
        private const string InvoicePaid = "invoice.paid";
        private const string SubscriptionDeleted = "customer.subscription.deleted";

        // TTL de idempotencia en Redis = tiempo máximo de reintento de Stripe (72h) + margen
        private static readonly TimeSpan IdempotencyTtl = TimeSpan.FromHours(24);

        private readonly ApplicationDbContext _dbContext;
        private readonly ISessionCache _sessionCache;
        private readonly ILogger<StripeWebhookService> _logger;

        public StripeWebhookService(
            ApplicationDbContext dbContext,
            ISessionCache sessionCache,
            ILogger<StripeWebhookService> logger)
        {
            _dbContext = dbContext;
            _sessionCache = sessionCache;
            _logger = logger;
        }

        public async Task<bool> HandleEventAsync(WebhookEventDto webhookEvent)
        {
            // ── 1ª BARRERA: Redis ─────────────────────────────────────────────────
            // Clave dedicada para idempotencia (namespace distinto al de sesiones).
            var idempotencyKey = $"webhook:processed:{webhookEvent.EventId}";
            var existingFlag = await _sessionCache.GetSessionAsync(idempotencyKey);

            if (existingFlag is not null)
            {
                _logger.LogInformation("Duplicate webhook rejected by Redis barrier. EventId: {EventId}", webhookEvent.EventId);
                return false; // Ya procesado: descartamos silenciosamente.
            }

            // Marcar el evento como "en proceso" en Redis ANTES de tocar la DB.
            // Si el proceso falla a mitad camino, expirará por TTL y se reintentará.
            await _sessionCache.SetSessionAsync(idempotencyKey, "1", IdempotencyTtl);

            // ── 2ª BARRERA + LÓGICA DE NEGOCIO: EF Core Transaction ───────────────
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Verificar en DB por si Redis fue vaciado (ej: restart de Redis).
                var alreadyPersisted = await _dbContext.PaymentEvents
                    .AnyAsync(e => e.ProviderEventId == webhookEvent.EventId);

                if (alreadyPersisted)
                {
                    _logger.LogInformation("Duplicate webhook rejected by DB barrier. EventId: {EventId}", webhookEvent.EventId);
                    await transaction.RollbackAsync();
                    return false;
                }

                // Procesar el evento según su tipo.
                await DispatchEventAsync(webhookEvent);

                // Confirmar la transacción solo si el dispatch fue exitoso.
                await transaction.CommitAsync();
                _logger.LogInformation("Webhook processed successfully. EventId: {EventId}, Type: {Type}", webhookEvent.EventId, webhookEvent.EventType);
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Quitar la flag de Redis para permitir reintento legítimo de Stripe.
                await _sessionCache.RemoveSessionAsync(idempotencyKey);
                _logger.LogError(ex, "Error processing webhook. EventId: {EventId}", webhookEvent.EventId);
                throw;
            }
        }

        // ── DISPATCHER DE EVENTOS ─────────────────────────────────────────────────

        private async Task DispatchEventAsync(WebhookEventDto webhookEvent)
        {
            switch (webhookEvent.EventType)
            {
                case InvoicePaid:
                    await HandleInvoicePaidAsync(webhookEvent);
                    break;
                case SubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(webhookEvent);
                    break;
                default:
                    // Evento no manejado: lo registramos pero sin lógica de negocio.
                    _logger.LogDebug("Unhandled Stripe event type: {Type}", webhookEvent.EventType);
                    await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "unhandled");
                    break;
            }
        }

        // ── HANDLERS ESPECÍFICOS ──────────────────────────────────────────────────

        private async Task HandleInvoicePaidAsync(WebhookEventDto webhookEvent)
        {
            // Extraer el customerId de Stripe para encontrar al jugador en nuestra DB.
            var customerId = ExtractStripeCustomerId(webhookEvent.DataObjectJson);
            var player = await FindPlayerByStripeCustomerIdAsync(customerId);

            if (player is null)
            {
                _logger.LogWarning("Player not found for Stripe customerId: {CustomerId}", customerId);
                await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "player_not_found");
                return;
            }

            // Aquí iría la lógica de provisión: activar rango, desbloquear cosméticos, etc.
            // Por ahora, registramos el evento y mantenemos la sesión activa.
            await PersistPaymentEventAsync(webhookEvent, player.Id, status: "paid");

            _logger.LogInformation("Invoice paid processed for PlayerId: {PlayerId}", player.Id);
        }

        private async Task HandleSubscriptionDeletedAsync(WebhookEventDto webhookEvent)
        {
            var customerId = ExtractStripeCustomerId(webhookEvent.DataObjectJson);
            var player = await FindPlayerByStripeCustomerIdAsync(customerId);

            if (player is null)
            {
                _logger.LogWarning("Player not found for Stripe customerId: {CustomerId}", customerId);
                await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "player_not_found");
                return;
            }

            // Registrar el evento de cancelación.
            await PersistPaymentEventAsync(webhookEvent, player.Id, status: "subscription_cancelled");

            // ── PURGA DE SESIÓN: Directiva 4 ─────────────────────────────────────
            // La suscripción fue cancelada → el usuario pierde sus privilegios.
            // Forzamos la expiración de la sesión en Redis para que el cliente
            // re-autentique y obtenga el estado actualizado sin privilegios.
            await _sessionCache.RemoveSessionAsync(player.Id.ToString());
            _logger.LogInformation("Session purged for PlayerId: {PlayerId} due to subscription cancellation.", player.Id);
        }

        // ── HELPERS ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Persiste el evento como registro de auditoría en la tabla payment_events.
        /// Este INSERT actúa como la segunda barrera de idempotencia gracias al índice UNIQUE.
        /// </summary>
        private async Task PersistPaymentEventAsync(WebhookEventDto webhookEvent, Guid? playerId, string status)
        {
            // Para eventos sin player conocido, usamos un PlayerId sentinel (Guid.Empty es válido como NULL-substitute).
            // Idealmente la columna sería nullable; por ahora usamos un Guid vacío si no hay player.
            var resolvedPlayerId = playerId ?? Guid.Empty;

            // Buscar si existe un player sentinel (el sistema puede tener un registro dummy).
            // En producción, PlayerId nullable sería la solución correcta.
            var paymentEvent = new PaymentEvent
            {
                Id = Guid.NewGuid(),
                Provider = PaymentProvider.STRIPE,
                ProviderEventId = webhookEvent.EventId,
                PaymentIntentId = ExtractPaymentIntentId(webhookEvent.DataObjectJson),
                PlayerId = resolvedPlayerId,
                Status = status,
                RawEvent = webhookEvent.DataObjectJson,
                ProcessedAt = DateTime.UtcNow
            };

            _dbContext.PaymentEvents.Add(paymentEvent);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Player?> FindPlayerByStripeCustomerIdAsync(string? customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return null;

            // El customerId de Stripe se almacena en ExternalAuth con Provider = "stripe".
            return await _dbContext.Players
                .Include(p => p.ExternalAuths)
                .FirstOrDefaultAsync(p => p.ExternalAuths.Any(
                    ea => ea.Provider == AuthProvider.STRIPE && ea.ExternalId == customerId));
        }

        private static string? ExtractStripeCustomerId(string dataObjectJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(dataObjectJson);
                if (doc.RootElement.TryGetProperty("customer", out var prop))
                    return prop.GetString();
            }
            catch { /* JSON malformado: ignorar */ }
            return null;
        }

        private static string ExtractPaymentIntentId(string dataObjectJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(dataObjectJson);
                if (doc.RootElement.TryGetProperty("payment_intent", out var prop))
                    return prop.GetString() ?? "unknown";
            }
            catch { /* JSON malformado: ignorar */ }
            return "unknown";
        }
    }
}
