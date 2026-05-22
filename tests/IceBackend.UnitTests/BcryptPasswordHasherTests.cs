using IceBackend.Application.Options;
using IceBackend.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IceBackend.UnitTests
{
    public class BcryptPasswordHasherTests
    {
        private readonly BcryptPasswordHasher _hasher;

        public BcryptPasswordHasherTests()
        {
            var mockOptions = new Mock<IOptionsSnapshot<AuthOptions>>();
            mockOptions.Setup(m => m.Value).Returns(new AuthOptions
            {
                BcryptWorkFactor = 4, // Rápido para tests
                LegacySalt = "IceLauncherSecretSalt"
            });
            _hasher = new BcryptPasswordHasher(mockOptions.Object);
        }

        [Fact]
        public void Hash_ShouldReturnBcryptString()
        {
            const string password = "mySecurePassword123";
            
            var hash = _hasher.Hash(password);
            
            Assert.NotNull(hash);
            Assert.StartsWith("$2", hash);
        }

        [Fact]
        public void Verify_ShouldReturnTrue_ForSamePassword()
        {
            const string password = "mySecurePassword123";
            var hash = _hasher.Hash(password);
            
            var isValid = _hasher.Verify(password, hash);
            
            Assert.True(isValid);
        }

        [Fact]
        public void Verify_ShouldReturnFalse_ForWrongPassword()
        {
            const string password = "mySecurePassword123";
            var hash = _hasher.Hash(password);
            
            var isValid = _hasher.Verify("wrongPassword", hash);
            
            Assert.False(isValid);
        }

        [Fact]
        public void IsLegacyHash_ShouldReturnTrue_ForSha256Hash()
        {
            const string legacyHash = "dGhpcyBpcyBhIHNoYS0yNTYgaGFzaA==";
            
            var isLegacy = _hasher.IsLegacyHash(legacyHash);
            
            Assert.True(isLegacy);
        }

        [Fact]
        public void IsLegacyHash_ShouldReturnFalse_ForBcryptHash()
        {
            var bcryptHash = _hasher.Hash("password");
            
            var isLegacy = _hasher.IsLegacyHash(bcryptHash);
            
            Assert.False(isLegacy);
        }

        [Fact]
        public void HashLegacy_ShouldMatchOldFormat()
        {
            const string password = "legacyPassword123";
            
            var hashLegacy = _hasher.HashLegacy(password);
            
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes("legacyPassword123IceLauncherSecretSalt");
            var expectedHash = System.Convert.ToBase64String(sha256.ComputeHash(bytes));

            Assert.Equal(expectedHash, hashLegacy);
        }
    }
}
