using IceBackend.Application.DTOs;

namespace IceBackend.Application.Interfaces
{
    /// <summary>
    /// Caso de uso principal para procesar un evento de webhook ya autenticado.
    /// Implementa la doble barrera de idempotencia y los event handlers transaccionales.
    /// </summary>
    public interface IStripeWebhookService
    {
        /// <summary>
        /// Procesa el evento. Retorna false si el evento ya fue procesado (idempotencia).
        /// </summary>
        Task<bool> HandleEventAsync(WebhookEventDto webhookEvent);
    }
}
