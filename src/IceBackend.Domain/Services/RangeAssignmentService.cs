using System;
using IceBackend.Domain.Entities;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio encargado de evaluar las invariantes para asignar rangos y firmar los tokens autorizados.
    /// </summary>
    public class RangeAssignmentService
    {
        private readonly IRangeTokenSigner _signer;

        public RangeAssignmentService(IRangeTokenSigner signer)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
        }

        public ValidatedRangeAssignmentToken ValidateAndSignRange(Player player, string rangeType, int durationDays, string eventId)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (string.IsNullOrWhiteSpace(rangeType)) throw new ArgumentException("El tipo de rango es requerido.", nameof(rangeType));
            if (durationDays <= 0) throw new ArgumentException("La duración del rango debe ser un entero positivo.", nameof(durationDays));
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("El ID del evento de pago es requerido.", nameof(eventId));

            // Validar que la expiración del rango sea calculada de manera inmutable
            var expiresAt = DateTime.UtcNow.AddDays(durationDays);

            // Firmar incluyendo el EventId para evitar replay attacks en la mutación del agregado
            byte[] signature = _signer.SignRange(player.Id, rangeType.ToUpperInvariant(), expiresAt, eventId);

            return new ValidatedRangeAssignmentToken(
                player.Id,
                rangeType.ToUpperInvariant(),
                expiresAt,
                eventId,
                signature
            );
        }
    }
}
