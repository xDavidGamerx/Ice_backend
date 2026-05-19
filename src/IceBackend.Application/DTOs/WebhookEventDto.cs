namespace IceBackend.Application.DTOs
{
    /// <summary>
    /// DTO que encapsula los datos relevantes de un evento de webhook ya validado.
    /// La capa de Application NUNCA ve el SDK de Stripe; solo trabaja con este DTO.
    /// </summary>
    public sealed class WebhookEventDto
    {
        /// <summary>Identificador único del evento en Stripe (ej: evt_1abc...).</summary>
        public string EventId { get; init; } = null!;

        /// <summary>Tipo del evento (ej: invoice.paid, customer.subscription.deleted).</summary>
        public string EventType { get; init; } = null!;

        /// <summary>El payload del objeto contenido en el evento (serializado como JSON).</summary>
        public string DataObjectJson { get; init; } = null!;

        /// <summary>Timestamp del evento según Stripe (UTC).</summary>
        public DateTime Created { get; init; }
    }
}
