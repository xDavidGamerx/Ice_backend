using System;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Certificado inmutable firmado criptográficamente que valida la asignación de un rango a un jugador.
    /// Amarrado de forma unívoca a un EventId de Stripe para prevenir replay attacks.
    /// </summary>
    public record ValidatedRangeAssignmentToken(
        PlayerId PlayerId,
        string RangeType,
        DateTime? ExpiresAt,
        string EventId, // EventId o StripeInvoiceId para unicidad absoluta
        byte[] Signature // Firma simétrica HMAC-SHA256
    );
}
