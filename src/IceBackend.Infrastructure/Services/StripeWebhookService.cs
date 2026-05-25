using System.Collections.Concurrent;
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
        private const string InvoicePaymentFailed = "invoice.payment_failed";
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
            var affectedPlayers = new ConcurrentBag<Guid>();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Verificar en DB por si Redis fue vaciado (ej: restart de Redis).
                var alreadyPersisted = await _dbContext.PaymentEvents
                    .AnyAsync(e => e.ProviderEventId == webhookEvent.EventId)
                    || await _dbContext.UnresolvedPaymentEvents
                    .AnyAsync(e => e.ProviderEventId == webhookEvent.EventId);

                if (alreadyPersisted)
                {
                    _logger.LogInformation("Duplicate webhook rejected by DB barrier. EventId: {EventId}", webhookEvent.EventId);
                    await transaction.RollbackAsync();
                    return false;
                }

                // Procesar el evento según su tipo y recolectar IDs mutados.
                await DispatchEventAsync(webhookEvent, affectedPlayers);

                // Confirmar la transacción solo si el dispatch fue exitoso.
                await transaction.CommitAsync();
                _logger.LogInformation("Webhook processed successfully. EventId: {EventId}, Type: {Type}", webhookEvent.EventId, webhookEvent.EventType);
                
                // Purga reactiva condicionada exclusivamente al COMMIT exitoso
                foreach (var playerId in affectedPlayers.Distinct())
                {
                    await _sessionCache.InvalidatePlayerSubscriptionAsync(playerId);
                    await _sessionCache.InvalidatePlayerCosmeticsAsync(playerId);
                }

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // ELIMINADO para prevenir enmascaramiento de excepciones:
                // await _sessionCache.RemoveSessionAsync(idempotencyKey);
                _logger.LogError(ex, "Error processing webhook. EventId: {EventId}", webhookEvent.EventId);
                throw;
            }
        }

        // ── DISPATCHER DE EVENTOS ─────────────────────────────────────────────────

        private async Task DispatchEventAsync(WebhookEventDto webhookEvent, ConcurrentBag<Guid> affectedPlayers)
        {
            switch (webhookEvent.EventType)
            {
                case InvoicePaid:
                    await HandleInvoicePaidAsync(webhookEvent, affectedPlayers);
                    break;
                case InvoicePaymentFailed:
                    await HandleInvoicePaymentFailedAsync(webhookEvent, affectedPlayers);
                    break;
                case SubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync(webhookEvent, affectedPlayers);
                    break;
                default:
                    // Evento no manejado: lo registramos pero sin lógica de negocio.
                    _logger.LogDebug("Unhandled Stripe event type: {Type}", webhookEvent.EventType);
                    await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "unhandled");
                    break;
            }
        }

        // ── HANDLERS ESPECÍFICOS ──────────────────────────────────────────────────

        private async Task HandleInvoicePaidAsync(WebhookEventDto webhookEvent, ConcurrentBag<Guid> affectedPlayers)
        {
            var customerId = ExtractStripeCustomerId(webhookEvent.DataObjectJson);
            var player = await FindPlayerByStripeCustomerIdAsync(customerId);

            if (player is null)
            {
                _logger.LogWarning("Player not found for Stripe customerId: {CustomerId}", customerId);
                await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "player_not_found");
                return;
            }

            await PersistPaymentEventAsync(webhookEvent, player.Id, status: "paid");

            // Lógica de Suscripción ICE+
            var stripeSubId = ExtractStripeSubscriptionId(webhookEvent.DataObjectJson);
            var billingReason = ExtractStripeBillingReason(webhookEvent.DataObjectJson);

            // Activar por defecto 30 días
            player.AssignIcePlusSubscription(stripeSubId, autoRenew: true, durationDays: 30, DateTime.UtcNow);

            // Incrementar meses acumulados si es una creación o renovación legítima de ciclo de suscripción
            if (billingReason == "subscription_create" || billingReason == "subscription_cycle")
            {
                player.IncrementIcePlusMonths(DateTime.UtcNow);
                _logger.LogInformation("Incremented accumulated subscription months for PlayerId: {PlayerId}", player.Id);
            }

            await _dbContext.SaveChangesAsync();

            // Registrar ID afectado para la purga post-commit
            affectedPlayers.Add(player.Id);

            _logger.LogInformation("Invoice paid processed and player scheduled for cache invalidation: {PlayerId}", player.Id);
        }

        private async Task HandleInvoicePaymentFailedAsync(WebhookEventDto webhookEvent, ConcurrentBag<Guid> affectedPlayers)
        {
            var customerId = ExtractStripeCustomerId(webhookEvent.DataObjectJson);
            var player = await FindPlayerByStripeCustomerIdAsync(customerId);

            if (player is null)
            {
                _logger.LogWarning("Player not found for Stripe customerId: {CustomerId}", customerId);
                await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "PENDING_RESOLUTION");
                return;
            }

            await PersistPaymentEventAsync(webhookEvent, player.Id, status: "payment_failed");

            // Desactivar suscripción activa por fallo de pago
            player.CancelIcePlusSubscription(DateTime.UtcNow);
            await _dbContext.SaveChangesAsync();

            // Registrar ID afectado para la purga post-commit
            affectedPlayers.Add(player.Id);
            
            _logger.LogWarning("Payment failed for PlayerId: {PlayerId}. Scheduled cache invalidation.", player.Id);
        }

        private async Task HandleSubscriptionDeletedAsync(WebhookEventDto webhookEvent, ConcurrentBag<Guid> affectedPlayers)
        {
            var customerId = ExtractStripeCustomerId(webhookEvent.DataObjectJson);
            var player = await FindPlayerByStripeCustomerIdAsync(customerId);

            if (player is null)
            {
                _logger.LogWarning("Player not found for Stripe customerId: {CustomerId}", customerId);
                await PersistPaymentEventAsync(webhookEvent, playerId: null, status: "player_not_found");
                return;
            }

            await PersistPaymentEventAsync(webhookEvent, player.Id, status: "subscription_cancelled");

            // Cancelar suscripción en PostgreSQL
            player.CancelIcePlusSubscription(DateTime.UtcNow);
            await _dbContext.SaveChangesAsync();

            // Registrar ID afectado para la purga post-commit
            affectedPlayers.Add(player.Id);
            
            _logger.LogInformation("Subscription cancelled for PlayerId: {PlayerId}. Scheduled cache invalidation.", player.Id);
        }

        // ── HELPERS ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Persiste el evento como registro de auditoría en la tabla payment_events.
        /// Este INSERT actúa como la segunda barrera de idempotencia gracias al índice UNIQUE.
        /// </summary>
        private async Task PersistPaymentEventAsync(WebhookEventDto webhookEvent, Guid? playerId, string status)
        {
            if (playerId == null)
            {
                var unresolvedEvent = new UnresolvedPaymentEvent(
                    Guid.NewGuid(),
                    PaymentProvider.STRIPE,
                    webhookEvent.EventId,
                    ExtractPaymentIntentId(webhookEvent.DataObjectJson),
                    status == "unhandled" ? "unhandled" : "PENDING_RESOLUTION",
                    webhookEvent.DataObjectJson
                );

                _dbContext.UnresolvedPaymentEvents.Add(unresolvedEvent);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Unresolved payment event persisted. EventId: {EventId}, Status: {Status}", webhookEvent.EventId, unresolvedEvent.Status);
            }
            else
            {
                var paymentEvent = new PaymentEvent(
                    Guid.NewGuid(),
                    PaymentProvider.STRIPE,
                    webhookEvent.EventId,
                    ExtractPaymentIntentId(webhookEvent.DataObjectJson),
                    playerId.Value,
                    status,
                    webhookEvent.DataObjectJson
                );

                _dbContext.PaymentEvents.Add(paymentEvent);
                await _dbContext.SaveChangesAsync();
            }
        }

        private async Task<Player?> FindPlayerByStripeCustomerIdAsync(string? customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return null;

            return await _dbContext.Players
                .Include(p => p.ExternalAuths)
                .Include(p => p.Subscription)
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

        private static string? ExtractStripeSubscriptionId(string dataObjectJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(dataObjectJson);
                if (doc.RootElement.TryGetProperty("subscription", out var prop))
                    return prop.GetString();
            }
            catch { }
            return null;
        }

        private static string? ExtractStripeBillingReason(string dataObjectJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(dataObjectJson);
                if (doc.RootElement.TryGetProperty("billing_reason", out var prop))
                    return prop.GetString();
            }
            catch { }
            return null;
        }
    }
}
