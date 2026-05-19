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
                .Options);

        private static IOptions<AuthOptions> DefaultAuthOptions() =>
            Options.Create(new AuthOptions { SessionTtlHours = 8 });

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
            return new AuthService(ctx, cacheMock.Object, DefaultAuthOptions());
        }

        // ── Register ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldGenerateNewUuidOnServer()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);

            var player = await sut.RegisterIceAccountAsync("testuser", "Password123!");

            Assert.NotEqual(Guid.Empty, player.Id);
            Assert.Equal("testuser", player.Username);
            Assert.Equal(UuidType.ICE, player.UuidType);

            var savedPlayer = await context.Players.FindAsync(player.Id);
            Assert.NotNull(savedPlayer);
        }

        [Fact]
        public async Task RegisterIceAccountAsync_ShouldInitializeCosmeticSlots()
        {
            using var context = GetInMemoryDbContext();
            var sut = BuildAuthService(context);
            int expectedSlotCount = Enum.GetValues(typeof(CosmeticType)).Length;

            var player = await sut.RegisterIceAccountAsync("user1", "pass1");

            Assert.Equal(expectedSlotCount, player.EquippedCosmetics.Count);
            Assert.All(player.EquippedCosmetics, c => Assert.Null(c.CosmeticId));
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
    }
}
