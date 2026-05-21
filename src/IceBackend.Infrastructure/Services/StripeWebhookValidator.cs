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

                return new WebhookEventDto
                {
                    EventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    DataObjectJson = stripeEvent.Data.RawObject?.ToString() ?? "{}",
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
