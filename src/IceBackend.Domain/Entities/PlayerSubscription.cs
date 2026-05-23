using System;

namespace IceBackend.Domain.Entities
{
    /// <summary>
    /// Entidad de Dominio que representa la suscripción ICE+ de un jugador.
    /// Mantiene el estado activo, la fecha de expiración, renovación e historial acumulado de meses
    /// para calcular los iconos evolutivos. Relación 1-a-1 con Player.
    /// </summary>
    public class PlayerSubscription
    {
        public Guid Id { get; private set; }
        public PlayerId PlayerId { get; private set; } = null!;
        public Player Player { get; private set; } = null!;
        public bool IsActive { get; private set; }
        public DateTime? ExpiresAt { get; private set; }
        public string? StripeSubscriptionId { get; private set; }
        public bool AutoRenew { get; private set; }
        public int AccumulatedMonths { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private PlayerSubscription() { } // Constructor privado para EF Core

        public PlayerSubscription(Guid id, PlayerId playerId)
        {
            if (id == Guid.Empty) throw new ArgumentException("El ID de la suscripción no puede estar vacío.", nameof(id));
            PlayerId = playerId ?? throw new ArgumentNullException(nameof(playerId));
            Id = id;
            IsActive = false;
            AccumulatedMonths = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Activa la suscripción estableciendo sus parámetros de vigencia.
        /// </summary>
        public void Activate(DateTime expiresAt, string? stripeSubscriptionId, bool autoRenew, DateTime currentTime)
        {
            IsActive = true;
            ExpiresAt = expiresAt;
            StripeSubscriptionId = stripeSubscriptionId;
            AutoRenew = autoRenew;
            UpdatedAt = currentTime;
        }

        /// <summary>
        /// Incrementa el contador acumulativo de meses activos para la progresión del icono.
        /// </summary>
        public void IncrementAccumulatedMonths(DateTime currentTime)
        {
            AccumulatedMonths++;
            UpdatedAt = currentTime;
        }

        /// <summary>
        /// Desactiva temporal o permanentemente la suscripción por cancelación o expiración de pago.
        /// </summary>
        public void Deactivate(DateTime currentTime)
        {
            IsActive = false;
            AutoRenew = false;
            UpdatedAt = currentTime;
        }

        /// <summary>
        /// Actualiza la fecha de expiración (por ejemplo, ante renovación del periodo).
        /// </summary>
        public void UpdateExpiration(DateTime expiresAt, DateTime currentTime)
        {
            ExpiresAt = expiresAt;
            UpdatedAt = currentTime;
        }
    }
}
