using System;
using System.Collections.Generic;
using IceBackend.Domain.Enums;
using IceBackend.Domain.Services;

namespace IceBackend.Domain.Entities
{
    public class Player
    {
        public PlayerId Id { get; private set; } = null!;
        public string Username { get; private set; } = null!;
        public UuidType UuidType { get; private set; }
        public string? PasswordHash { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Suscripción ICE+ y Reconciliación eventual de consistencia
        public PlayerSubscription? Subscription { get; private set; }
        public bool RequiresSessionSync { get; private set; }

        // Propiedades públicas de dominio (Claves no enumerables UUID)
        public CosmeticId? EquippedHatId { get; private set; }
        public CosmeticId? EquippedWingId { get; private set; }
        public CosmeticId? EquippedCapeId { get; private set; }
        public CosmeticId? EquippedShirtId { get; private set; }
        public CosmeticId? EquippedPantsId { get; private set; }
        public CosmeticId? EquippedShoesId { get; private set; }

        // Campos de respaldo de base de datos (Mapeados de forma privada por EF Core en la infraestructura)
        private int? _equippedHatInternalId;
        private int? _equippedWingInternalId;
        private int? _equippedCapeInternalId;
        private int? _equippedShirtInternalId;
        private int? _equippedPantsInternalId;
        private int? _equippedShoesInternalId;

        private readonly List<ExternalAuth> _externalAuths = new();
        public IReadOnlyCollection<ExternalAuth> ExternalAuths => _externalAuths.AsReadOnly();

        private readonly List<BootstrapToken> _bootstrapTokens = new();
        public IReadOnlyCollection<BootstrapToken> BootstrapTokens => _bootstrapTokens.AsReadOnly();

        private readonly List<PlayerCosmeticOwnership> _cosmeticOwnerships = new();
        public IReadOnlyCollection<PlayerCosmeticOwnership> CosmeticOwnerships => _cosmeticOwnerships.AsReadOnly();

        private readonly List<PaymentEvent> _paymentEvents = new();
        public IReadOnlyCollection<PaymentEvent> PaymentEvents => _paymentEvents.AsReadOnly();

        private Player() { } // Constructor para EF Core

        public Player(PlayerId id, string username, UuidType uuidType, string? passwordHash)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Username = !string.IsNullOrWhiteSpace(username) 
                ? username 
                : throw new ArgumentException("El nombre de usuario no puede estar vacío.", nameof(username));
            UuidType = uuidType;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
            RequiresSessionSync = false;
        }

        public void AddExternalAuth(ExternalAuth auth)
        {
            if (auth == null) throw new ArgumentNullException(nameof(auth));
            _externalAuths.Add(auth);
        }

        public void UpdatePasswordHash(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("El hash de la contraseña no puede estar vacío.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
        }

        // --- GESTIÓN DE SUSCRIPCIÓN ICE+ ---

        /// <summary>
        /// Asigna y activa la suscripción ICE+ del jugador.
        /// </summary>
        public void AssignIcePlusSubscription(string? stripeSubscriptionId, bool autoRenew, int durationDays, DateTime currentTime)
        {
            if (durationDays <= 0)
                throw new ArgumentException("La duración de la suscripción debe ser un número positivo de días.", nameof(durationDays));

            var expiresAt = currentTime.AddDays(durationDays);

            if (Subscription == null)
            {
                Subscription = new PlayerSubscription(Guid.NewGuid(), Id);
            }

            Subscription.Activate(expiresAt, stripeSubscriptionId, autoRenew, currentTime);
            RequiresSessionSync = true;
        }

        /// <summary>
        /// Incrementa el acumulador de meses de suscripción activa de la cuenta.
        /// </summary>
        public void IncrementIcePlusMonths(DateTime currentTime)
        {
            if (Subscription == null)
            {
                Subscription = new PlayerSubscription(Guid.NewGuid(), Id);
            }

            Subscription.IncrementAccumulatedMonths(currentTime);
            RequiresSessionSync = true;
        }

        /// <summary>
        /// Cancela la suscripción activa (la marca como inactiva de inmediato).
        /// </summary>
        public void CancelIcePlusSubscription(DateTime currentTime)
        {
            if (Subscription != null)
            {
                Subscription.Deactivate(currentTime);
                RequiresSessionSync = true;
            }
        }

        public void ClearSessionSync()
        {
            RequiresSessionSync = false;
        }

        // --- MÉTODOS DE MUTACIÓN EXPRESIVOS DE WEARABLES ---

        public void Equip(ValidatedEquipmentToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            switch (token.Slot)
            {
                case CosmeticType.HAT:
                    EquipHat(token);
                    break;
                case CosmeticType.WING:
                    EquipWing(token);
                    break;
                case CosmeticType.CAPE:
                    EquipCape(token);
                    break;
                case CosmeticType.SHIRT:
                    EquipShirt(token);
                    break;
                case CosmeticType.PANTS:
                    EquipPants(token);
                    break;
                case CosmeticType.SHOES:
                    EquipShoes(token);
                    break;
                default:
                    throw new InvalidOperationException($"Ranura de cosmético no soportada: {token.Slot}");
            }
        }

        public void Unequip(CosmeticType slot)
        {
            switch (slot)
            {
                case CosmeticType.HAT:
                    UnequipHat();
                    break;
                case CosmeticType.WING:
                    UnequipWing();
                    break;
                case CosmeticType.CAPE:
                    UnequipCape();
                    break;
                case CosmeticType.SHIRT:
                    UnequipShirt();
                    break;
                case CosmeticType.PANTS:
                    UnequipPants();
                    break;
                case CosmeticType.SHOES:
                    UnequipShoes();
                    break;
                default:
                    throw new InvalidOperationException($"Ranura de cosmético no soportada: {slot}");
            }
        }

        public void EquipHat(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.HAT);
            EquippedHatId = token.CosmeticId;
            _equippedHatInternalId = token.CosmeticInternalId;
        }

        public void UnequipHat()
        {
            EquippedHatId = null;
            _equippedHatInternalId = null;
        }

        public void EquipWing(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.WING);
            EquippedWingId = token.CosmeticId;
            _equippedWingInternalId = token.CosmeticInternalId;
        }

        public void UnequipWing()
        {
            EquippedWingId = null;
            _equippedWingInternalId = null;
        }

        public void EquipCape(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.CAPE);
            EquippedCapeId = token.CosmeticId;
            _equippedCapeInternalId = token.CosmeticInternalId;
        }

        public void UnequipCape()
        {
            EquippedCapeId = null;
            _equippedCapeInternalId = null;
        }

        public void EquipShirt(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.SHIRT);
            EquippedShirtId = token.CosmeticId;
            _equippedShirtInternalId = token.CosmeticInternalId;
        }

        public void UnequipShirt()
        {
            EquippedShirtId = null;
            _equippedShirtInternalId = null;
        }

        public void EquipPants(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.PANTS);
            EquippedPantsId = token.CosmeticId;
            _equippedPantsInternalId = token.CosmeticInternalId;
        }

        public void UnequipPants()
        {
            EquippedPantsId = null;
            _equippedPantsInternalId = null;
        }

        public void EquipShoes(ValidatedEquipmentToken token)
        {
            ValidateToken(token, CosmeticType.SHOES);
            EquippedShoesId = token.CosmeticId;
            _equippedShoesInternalId = token.CosmeticInternalId;
        }

        public void UnequipShoes()
        {
            EquippedShoesId = null;
            _equippedShoesInternalId = null;
        }

        // --- VALIDACIONES DE CONSISTENCIA DE NEGOCIO ---

        private void ValidateToken(ValidatedEquipmentToken token, CosmeticType expectedSlot)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            // 1. Validar expiración (Prevención de Replay Attacks)
            if (DateTime.UtcNow > token.ExpiredAt)
                throw new InvalidOperationException("El token de equipamiento ha expirado.");

            // 2. Garantizar consistencia de jugador y slot
            if (token.PlayerId != Id)
                throw new InvalidOperationException("El token pertenece a otro jugador.");

            if (token.Slot != expectedSlot)
                throw new InvalidOperationException($"El token no corresponde a la ranura '{expectedSlot}'.");
        }
    }
}
