using IceBackend.Application.DTOs;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Contrato para el servicio de validación de webhooks de Stripe.
    /// La implementación (con el SDK real de Stripe) vive en Infrastructure.
    /// </summary>
    public interface IStripeWebhookValidator
    {
        /// <summary>
        /// Valida la firma del webhook usando el Stripe-Signature header y el WebhookSecret.
        /// </summary>
        /// <param name="rawBody">El body crudo de la petición HTTP (necesario para el hash).</param>
        /// <param name="signatureHeader">El valor del header 'Stripe-Signature'.</param>
        /// <returns>El evento deserializado como DTO, o null si la firma es inválida.</returns>
        WebhookEventDto? ValidateAndConstruct(string rawBody, string signatureHeader);
    }
}
