using System;
using System.Security.Cryptography;
using System.Text;
using IceBackend.Application.Options;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Services;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class RangeTokenSigner : IRangeTokenSigner
    {
        private readonly CdnOptions _options;

        public RangeTokenSigner(IOptionsSnapshot<CdnOptions> options)
        {
            _options = options.Value;
        }

        public byte[] SignRange(PlayerId playerId, string rangeType, DateTime? expiresAt, string eventId)
        {
            if (playerId == null) throw new ArgumentNullException(nameof(playerId));
            if (string.IsNullOrWhiteSpace(rangeType)) throw new ArgumentException("El tipo de rango no puede estar vacío.", nameof(rangeType));
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("El ID del evento de pago no puede estar vacío.", nameof(eventId));

            var payload = GetCanonicalPayload(playerId, rangeType, expiresAt, eventId);
            var signingKey = GetSigningKey(playerId.Value);

            using var hmac = new HMACSHA256(signingKey);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        }

        public bool VerifyRange(ValidatedRangeAssignmentToken token)
        {
            if (token == null || token.Signature == null) return false;

            var expectedSignature = SignRange(token.PlayerId, token.RangeType, token.ExpiresAt, token.EventId);

            return CryptographicOperations.FixedTimeEquals(token.Signature, expectedSignature);
        }

        private string GetCanonicalPayload(PlayerId playerId, string rangeType, DateTime? expiresAt, string eventId)
        {
            var expirationTicks = expiresAt.HasValue ? expiresAt.Value.Ticks.ToString() : "never";
            return $"{playerId.Value}:{rangeType.ToUpperInvariant()}:{expirationTicks}:{eventId}";
        }

        private byte[] GetSigningKey(Guid playerId)
        {
            var baseSecret = string.IsNullOrEmpty(_options.SigningSecret)
                ? "default-super-secret-backup-signing-key-ice-launcher"
                : _options.SigningSecret;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes($"{baseSecret}:range:{playerId}");
            return sha256.ComputeHash(bytes);
        }
    }
}
