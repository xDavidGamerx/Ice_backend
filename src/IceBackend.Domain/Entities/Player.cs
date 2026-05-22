using System;
using System.Collections.Generic;
using System.Linq;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; } = null!;
        public UuidType UuidType { get; private set; }
        public string? PasswordHash { get; private set; }
        public string? SessionHash { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private readonly List<PlayerCosmetic> _equippedCosmetics = new();
        public IReadOnlyCollection<PlayerCosmetic> EquippedCosmetics => _equippedCosmetics.AsReadOnly();

        private readonly List<ExternalAuth> _externalAuths = new();
        public IReadOnlyCollection<ExternalAuth> ExternalAuths => _externalAuths.AsReadOnly();

        private readonly List<BootstrapToken> _bootstrapTokens = new();
        public IReadOnlyCollection<BootstrapToken> BootstrapTokens => _bootstrapTokens.AsReadOnly();

        private readonly List<PlayerCosmeticOwnership> _cosmeticOwnerships = new();
        public IReadOnlyCollection<PlayerCosmeticOwnership> CosmeticOwnerships => _cosmeticOwnerships.AsReadOnly();

        private readonly List<PaymentEvent> _paymentEvents = new();
        public IReadOnlyCollection<PaymentEvent> PaymentEvents => _paymentEvents.AsReadOnly();

        private Player() { } // Constructor for EF Core

        public Player(Guid id, string username, UuidType uuidType, string? passwordHash)
        {
            if (id == Guid.Empty) throw new ArgumentException("Player ID cannot be empty.", nameof(id));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username cannot be empty.", nameof(username));

            Id = id;
            Username = username;
            UuidType = uuidType;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;

            InitializeCosmeticSlots();
        }

        private void InitializeCosmeticSlots()
        {
            var cosmeticSlots = Enum.GetValues(typeof(CosmeticType)).Cast<CosmeticType>();
            foreach (var slot in cosmeticSlots)
            {
                _equippedCosmetics.Add(new PlayerCosmetic(Id, slot));
            }
        }

        public void AddExternalAuth(ExternalAuth auth)
        {
            if (auth == null) throw new ArgumentNullException(nameof(auth));
            _externalAuths.Add(auth);
        }

        /// <summary>
        /// Actualiza de forma segura el hash de la contraseña utilizando un formato más robusto (BCrypt).
        /// </summary>
        public void UpdatePasswordHash(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
        }

        /// <summary>
        /// Equipa un cosmético en el slot correspondiente.
        /// El cosmético debe ser propiedad del jugador (validado externamente por el UseCase).
        /// </summary>
        public void EquipCosmetic(CosmeticType slot, Guid cosmeticId)
        {
            if (cosmeticId == Guid.Empty) throw new ArgumentException("Cosmetic ID cannot be empty.", nameof(cosmeticId));

            var playerCosmetic = _equippedCosmetics.FirstOrDefault(c => c.Slot == slot)
                ?? throw new InvalidOperationException($"Slot '{slot}' not found for player '{Id}'.");

            playerCosmetic.Equip(cosmeticId);
        }

        /// <summary>
        /// Desequipa el cosmético del slot indicado, dejándolo vacío.
        /// </summary>
        public void UnequipCosmetic(CosmeticType slot)
        {
            var playerCosmetic = _equippedCosmetics.FirstOrDefault(c => c.Slot == slot)
                ?? throw new InvalidOperationException($"Slot '{slot}' not found for player '{Id}'.");

            playerCosmetic.Unequip();
        }
    }
}
