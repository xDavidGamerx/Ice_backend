using System;
using System.Security.Cryptography;
using System.Text;
using IceBackend.Application.Options;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Services;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class CosmeticTokenSigner : ICosmeticTokenSigner
    {
        private readonly CdnOptions _options;

        public CosmeticTokenSigner(IOptionsSnapshot<CdnOptions> options)
        {
            _options = options.Value;
        }

        public byte[] Sign(ValidatedEquipmentToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            var payload = GetCanonicalPayload(token);
            var signingKey = GetSigningKey(token.PlayerId.Value);

            using var hmac = new HMACSHA256(signingKey);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        }

        public bool Verify(ValidatedEquipmentToken token)
        {
            if (token == null || token.Signature == null) return false;

            var expectedSignature = Sign(token);

            // Comparación en tiempo constante para mitigar ataques de temporización
            return CryptographicOperations.FixedTimeEquals(token.Signature, expectedSignature);
        }

        private string GetCanonicalPayload(ValidatedEquipmentToken token)
        {
            // Formato de texto canónico e inmutable para el cálculo del hash
            return $"{token.PlayerId.Value}:{token.CosmeticId.Value}:{token.Slot}:{token.CosmeticInternalId}:{token.AssetVersion}:{token.ExpiredAt.Ticks}";
        }

        private byte[] GetSigningKey(Guid playerId)
        {
            var baseSecret = string.IsNullOrEmpty(_options.SigningSecret)
                ? "default-super-secret-backup-signing-key-ice-launcher"
                : _options.SigningSecret;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{baseSecret}:cosmetic:{playerId}");
            return sha256.ComputeHash(bytes);
        }
    }
}
