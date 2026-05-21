using System;
using System.Security.Cryptography;
using System.Text;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class CdnUrlSigner : ICdnUrlSigner
    {
        private readonly CdnOptions _options;

        public CdnUrlSigner(IOptionsSnapshot<CdnOptions> options)
        {
            _options = options.Value;
        }

        public (string Url, DateTime ExpiresAt) GeneratePresignedUrl(string assetHash)
        {
            var expiresAt = DateTime.UtcNow.AddMinutes(1);
            var expiryTimestamp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds();

            // Simulación de URL prefirmada (ej. HMAC SHA256)
            var stringToSign = $"{assetHash}:{expiryTimestamp}";
            string signature;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningSecret)))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                signature = Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
            }

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var signedUrl = $"{baseUrl}/assets/{assetHash}?expires={expiryTimestamp}&sig={signature}";

            return (signedUrl, expiresAt);
        }
    }
}
