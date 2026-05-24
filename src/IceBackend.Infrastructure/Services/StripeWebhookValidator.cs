using IceBackend.Application.DTOs;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace IceBackend.Infrastructure.Services
{
    /// <summary>
    /// Valida la firma criptográfica del webhook usando el SDK oficial de Stripe.
    /// Esta clase es la ÚNICA que conoce Stripe en todo el sistema.
    /// </summary>
    public class StripeWebhookValidator : IStripeWebhookValidator
    {
        private readonly string _webhookSecret;
        private readonly ILogger<StripeWebhookValidator> _logger;

        public StripeWebhookValidator(
            IOptionsSnapshot<StripeOptions> stripeOptions,
            ILogger<StripeWebhookValidator> logger)
        {
            _webhookSecret = stripeOptions.Value.WebhookSecret;
            _logger = logger;
        }

        public WebhookEventDto? ValidateAndConstruct(string rawBody, string signatureHeader)
        {
            try
            {
                // ConstructEvent lanza StripeException si la firma no coincide.
                // Actúa como la barrera criptográfica en el borde de la API.
                var stripeEvent = EventUtility.ConstructEvent(
                    rawBody,
                    signatureHeader,
                    _webhookSecret,
                    throwOnApiVersionMismatch: false);

                // Evitar pérdida de propiedades en el mapeo estricto del SDK (Stripe.net).
                // Extraemos el subnodo data.object directamente del JSON original y en texto plano.
                var rawObjectJson = "{}";
                try 
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawBody);
                    if (doc.RootElement.TryGetProperty("data", out var dataProp) && 
                        dataProp.TryGetProperty("object", out var objectProp))
                    {
                        rawObjectJson = objectProp.GetRawText();
                    }
                }
                catch { }

                return new WebhookEventDto
                {
                    EventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    DataObjectJson = rawObjectJson,
                    Created = stripeEvent.Created
                };
            }
            catch (StripeException ex)
            {
                _logger.LogWarning("Stripe webhook signature validation failed: {Message}", ex.Message);
                return null;
            }
        }
    }
}
