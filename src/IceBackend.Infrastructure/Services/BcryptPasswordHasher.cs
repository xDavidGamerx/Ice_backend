using System;
using System.Security.Cryptography;
using System.Text;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        private readonly AuthOptions _options;

        public BcryptPasswordHasher(IOptionsSnapshot<AuthOptions> options)
        {
            _options = options.Value;
        }

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _options.BcryptWorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        public bool IsLegacyHash(string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            return !hash.StartsWith('$');
        }

        public string HashLegacy(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + _options.LegacySalt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
