using System;
using System.Security.Cryptography;
using System.Text;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class AssetTokenService : IAssetTokenService
    {
        private readonly CdnOptions _cdnOptions;

        public AssetTokenService(IOptionsSnapshot<CdnOptions> cdnOptions)
        {
            _cdnOptions = cdnOptions.Value;
        }

        public string GenerateDeliveryToken(Guid playerId, string hash, TimeSpan ttl)
        {
            var expirationTicks = DateTime.UtcNow.Add(ttl).Ticks;
            var payload = $"{playerId}:{hash}:{expirationTicks}";
            
            var payloadBase64 = Base64UrlEncode(payload);
            var signingKey = GetSigningKey(playerId);

            using var hmac = new HMACSHA256(signingKey);
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
            var signatureBase64 = Base64UrlEncode(signatureBytes);

            return $"{payloadBase64}.{signatureBase64}";
        }

        public bool ValidateDeliveryToken(string token, string expectedHash, out Guid playerId)
        {
            playerId = Guid.Empty;

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var parts = token.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var payloadBase64 = parts[0];
            var signatureBase64 = parts[1];

            try
            {
                var payload = Base64UrlDecode(payloadBase64);
                var segments = payload.Split(':');
                if (segments.Length != 3)
                {
                    return false;
                }

                if (!Guid.TryParse(segments[0], out var tokenPlayerId))
                {
                    return false;
                }

                var tokenHash = segments[1];
                if (!long.TryParse(segments[2], out var expirationTicks))
                {
                    return false;
                }

                // 1. Validar que no haya expirado
                if (DateTime.UtcNow.Ticks > expirationTicks)
                {
                    return false;
                }

                // 2. Validar que corresponda al hash esperado
                if (!string.Equals(tokenHash, expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // 3. Validar la firma HMAC usando la clave derivada de este PlayerId
                var signingKey = GetSigningKey(tokenPlayerId);
                using var hmac = new HMACSHA256(signingKey);
                var expectedSignatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadBase64));
                var expectedSignatureBase64 = Base64UrlEncode(expectedSignatureBytes);

                // Comparación en tiempo constante para evitar ataques de temporización
                if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(signatureBase64), 
                    Encoding.UTF8.GetBytes(expectedSignatureBase64)))
                {
                    return false;
                }

                playerId = tokenPlayerId;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] GetSigningKey(Guid playerId)
        {
            var baseSecret = string.IsNullOrEmpty(_cdnOptions.SigningSecret)
                ? "default-super-secret-backup-signing-key-ice-launcher"
                : _cdnOptions.SigningSecret;

            // Derivar una clave única por jugador combinando el secreto global y el PlayerId (Salt/Pepper)
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{baseSecret}:{playerId}");
            return sha256.ComputeHash(bytes);
        }

        private static string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Base64UrlEncode(bytes);
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private static string Base64UrlDecode(string input)
        {
            var base64 = input
                .Replace("-", "+")
                .Replace("_", "/");

            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
