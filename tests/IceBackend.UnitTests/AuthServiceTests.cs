using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using IceBackend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IceBackend.UnitTests
{
    public class AuthServiceTests
    {
        // ── Helpers ─────────────────────────────────────────────────────────────

        private static ApplicationDbContext GetInMemoryDbContext() =>
            new(new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options);

        private static IOptionsSnapshot<AuthOptions> DefaultAuthOptions()
        {
            var mock = new Mock<IOptionsSnapshot<AuthOptions>>();
            mock.Setup(m => m.Value).Returns(new AuthOptions 
            { 
                SessionTtlHours = 8,
                BcryptWorkFactor = 4, // Rápido para tests
                LegacySalt = "IceLauncherSecretSalt"
            });
            return mock.Object;
        }

        /// <summary>
        /// Retorna un mock de ISessionCache configurado para no hacer nada por defecto.
        /// Expone el Mock para que los tests puedan verificar llamadas.
        /// </summary>
        private static Mock<ISessionCache> BuildSessionCacheMock()
        {
            var mock = new Mock<ISessionCache>();
            // SetSessionAsync no hace nada (estado en memoria; no hay Redis real en tests).
            mock.Setup(m => m.SetSessionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            mock.Setup(m => m.GetSessionAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            mock.Setup(m => m.RemoveSessionAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        private static AuthService BuildAuthService(ApplicationDbContext ctx, Mock<ISessionCache>? cacheMock = null)
        {
            cacheMock ??= BuildSessionCacheMock();
            var options = DefaultAuthOptions();
            var hasher = new BcryptPasswordHasher(options);
            return new AuthService(ctx, cacheMock.Object, options, hasher, new Mock<Microsoft.Extensions.Logging.ILogger<AuthService>>().Object);
        }

        // ── Register ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldGenerateNewUuidOnServer()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            var player = await sut.RegisterIceAccountAsync("testuser", "Password123!");

            Assert.NotEqual(Guid.Empty, player.Id.Value);
            Assert.Equal("testuser", player.Username);
            Assert.Equal(UuidType.ICE, player.UuidType);

            var savedPlayer = await context.Players.FindAsync(player.Id);
            Assert.NotNull(savedPlayer);
        }

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldInitializeCosmeticSlotsToNull()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
 
            var player = await sut.RegisterIceAccountAsync("user1", "pass1");
 
            Assert.Null(player.EquippedHatId);
            Assert.Null(player.EquippedWingId);
            Assert.Null(player.EquippedCapeId);
            Assert.Null(player.EquippedShirtId);
            Assert.Null(player.EquippedPantsId);
            Assert.Null(player.EquippedShoesId);
        }

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldNotStorePlainPassword()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
            const string password = "SuperSecretPassword";

            var player = await sut.RegisterIceAccountAsync("secureUser", password);

            Assert.NotEqual(password, player.PasswordHash);
            Assert.True(player.PasswordHash!.Length > 20);
        }

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldThrow_WhenUsernameAlreadyTaken()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
            await sut.RegisterIceAccountAsync("duplicateUser", "pass1");

            await Assert.ThrowsAsync<Exception>(() =>
                sut.RegisterIceAccountAsync("duplicateUser", "pass2"));
        }

        // ── Login ────────────────────────────────────────────────────────────────

        [Fact]
        public async Task LoginIceAccountAsync_ShouldReturnPlayerAndToken_WhenCredentialsAreValid()
        {
            using var context = GetInMemoryDbContext();
            var cacheMock = BuildSessionCacheMock();
            var sut = BuildAuthService(context, cacheMock);

            await sut.RegisterIceAccountAsync("loginUser", "CorrectPassword");
            var result = await sut.LoginIceAccountAsync("loginUser", "CorrectPassword");

            Assert.NotNull(result);
            Assert.Equal("loginUser", result!.Value.Player.Username);
            Assert.NotEmpty(result.Value.SessionToken);
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldPersistSessionInCache_WhenCredentialsAreValid()
        {
            using var context = GetInMemoryDbContext();
            var cacheMock = BuildSessionCacheMock();
            var sut = BuildAuthService(context, cacheMock);

            await sut.RegisterIceAccountAsync("cacheUser", "SecurePass");
            var result = await sut.LoginIceAccountAsync("cacheUser", "SecurePass");

            // Verificar que ISessionCache.SetSessionAsync fue invocado exactamente una vez
            // con el UUID del jugador como clave.
            cacheMock.Verify(m =>
                m.SetSessionAsync(
                    result!.Value.Player.Id.ToString(),
                    result.Value.SessionToken,
                    TimeSpan.FromHours(8)),
                Times.Once);
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
            await sut.RegisterIceAccountAsync("wrongPassUser", "RightPassword");

            var result = await sut.LoginIceAccountAsync("wrongPassUser", "WrongPassword");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            var result = await sut.LoginIceAccountAsync("nonexistent", "AnyPassword");

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldGenerateUniqueTokensOnEachCall()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
            await sut.RegisterIceAccountAsync("tokenUser", "pass");

            var result1 = await sut.LoginIceAccountAsync("tokenUser", "pass");
            var result2 = await sut.LoginIceAccountAsync("tokenUser", "pass");

            Assert.NotEqual(result1!.Value.SessionToken, result2!.Value.SessionToken);
        }

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldStoreBcryptHash()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            var player = await sut.RegisterIceAccountAsync("bcryptUser", "mySecurePassword123");

            Assert.NotNull(player.PasswordHash);
            Assert.StartsWith("$2", player.PasswordHash); // BCrypt prefix
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldSucceedAndRehash_WhenLegacyUserLogsIn()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            // Crear manualmente un usuario con hash legacy SHA-256
            const string username = "legacyUser";
            const string password = "legacyPassword123";
            var legacyHash = HashLegacyPasswordInTest(password);

            var player = new Player(Guid.NewGuid(), username, UuidType.ICE, legacyHash);
            context.Players.Add(player);
            await context.SaveChangesAsync();

            // Intentar login: debe tener éxito y migrar a BCrypt de forma transparente
            var loginResult = await sut.LoginIceAccountAsync(username, password);

            Assert.NotNull(loginResult);
            
            // Recargar de la DB para verificar el nuevo hash
            var updatedPlayer = await context.Players.FindAsync(player.Id);
            Assert.NotNull(updatedPlayer);
            Assert.NotNull(updatedPlayer.PasswordHash);
            Assert.StartsWith("$2", updatedPlayer.PasswordHash); // Verificamos que es BCrypt ahora

            // Segundo login: debe continuar funcionando ahora usando validación BCrypt nativa
            var secondLoginResult = await sut.LoginIceAccountAsync(username, password);
            Assert.NotNull(secondLoginResult);
        }

        [Fact]
        public async Task LoginIceAccountAsync_ShouldNotRehash_WhenBcryptUserLogsIn()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            var player = await sut.RegisterIceAccountAsync("alreadyBcryptUser", "password123");
            var originalHash = player.PasswordHash;

            var loginResult = await sut.LoginIceAccountAsync("alreadyBcryptUser", "password123");
            Assert.NotNull(loginResult);

            // Recargar de la DB
            var reloadedPlayer = await context.Players.FindAsync(player.Id);
            Assert.NotNull(reloadedPlayer);
            Assert.Equal(originalHash, reloadedPlayer.PasswordHash); // No debió cambiar el hash
        }

        private static string HashLegacyPasswordInTest(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password + "IceLauncherSecretSalt");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
