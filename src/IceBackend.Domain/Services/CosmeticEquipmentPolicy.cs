using System;
using System.Linq;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;

namespace IceBackend.Domain.Services
{
    /// <summary>
    /// Servicio de Dominio encargado de evaluar las invariantes de equipamiento
    /// cruzando el estado del jugador y la configuración de versiones del catálogo.
    /// </summary>
    public class CosmeticEquipmentPolicy
    {
        private readonly ICosmeticTokenSigner _signer;
        private readonly IMojangSessionValidator _mojangValidator;

        public CosmeticEquipmentPolicy(ICosmeticTokenSigner signer, IMojangSessionValidator mojangValidator)
        {
            _signer = signer ?? throw new ArgumentNullException(nameof(signer));
            _mojangValidator = mojangValidator ?? throw new ArgumentNullException(nameof(mojangValidator));
        }

        public async Task<ValidatedEquipmentToken> ValidateAndSignEquipmentAsync(
            Player player, 
            CosmeticAsset asset, 
            AssetArchitecture clientArchitecture,
            string? mojangAccessToken)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            // 1. Validar compatibilidad de versión física del catálogo
            bool hasCompatibleVersion = asset.Versions.Any(v => 
                v.Architecture == clientArchitecture || v.Architecture == AssetArchitecture.Universal);

            if (!hasCompatibleVersion)
                throw new InvalidOperationException($"El cosmético '{asset.DisplayName}' no es compatible con el cliente: {clientArchitecture}.");

            // 2. Control de identidades y validación Mojang Session API
            // Las cuentas ICE (no premium) y PREMIUM (premium) pueden usar arquitectura Modern o Legacy.
            // Si el jugador es Premium y se proporciona un token de sesión de Mojang, lo validamos.
            if (player.UuidType == UuidType.PREMIUM && !string.IsNullOrWhiteSpace(mojangAccessToken))
            {
                // Validación directa de infraestructura contra servidores oficiales de Mojang
                bool isSessionValid = await _mojangValidator.ValidateSessionAsync(player.Username, mojangAccessToken);
                if (!isSessionValid)
                    throw new InvalidOperationException("La sesión Premium de Mojang no pudo ser validada.");
            }

            // 3. TTL del token (ej. Expira en 5 minutos para prevenir Replay Attacks)
            DateTime expiredAt = DateTime.UtcNow.AddMinutes(5);

            // 4. Instanciar token temporal crudo (sin firma)
            var unsignedToken = new ValidatedEquipmentToken(
                player.Id, 
                asset.Id, 
                asset.CosmeticType,
                asset.InternalId,
                asset.AssetVersion,
                expiredAt,
                Array.Empty<byte>() // Aún sin firma
            );

            // 5. Criptógrafo firma el payload estructurado del token de forma determinista
            byte[] signature = _signer.Sign(unsignedToken);

            // Retornamos el token firmado
            return unsignedToken with { Signature = signature };
        }
    }
}
