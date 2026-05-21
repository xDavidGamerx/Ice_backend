using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Application.Options;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IceBackend.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISessionCache _sessionCache;
        private readonly TimeSpan _sessionTtl;

        public AuthService(
            ApplicationDbContext dbContext,
            ISessionCache sessionCache,
            IOptionsSnapshot<AuthOptions> authOptions)
        {
            _dbContext = dbContext;
            _sessionCache = sessionCache;
            _sessionTtl = TimeSpan.FromHours(authOptions.Value.SessionTtlHours);
        }

        public async Task<Player> RegisterIceAccountAsync(string username, string password)
        {
            var existingPlayer = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower());

            if (existingPlayer != null)
            {
                throw new Exception("Username is already taken.");
            }

            // Regla 1 y 2: El UUID se genera en el servidor de forma aislada. No se acepta del cliente.
            var newPlayer = new Player(
                Guid.NewGuid(),
                username,
                UuidType.ICE,
                HashPassword(password)
            );

            _dbContext.Players.Add(newPlayer);
            await _dbContext.SaveChangesAsync();

            return newPlayer;
        }

        public async Task<(Player Player, string SessionToken)?> LoginIceAccountAsync(string username, string password)
        {
            var player = await _dbContext.Players
                .FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower() && p.UuidType == UuidType.ICE);

            if (player == null)
            {
                return null;
            }

            if (player.PasswordHash != HashPassword(password))
            {
                return null;
            }

            // Generar token de sesión de forma segura en el servidor (nunca viene del cliente).
            var sessionToken = GenerateSecureToken();

            // Persistir en Redis de forma atómica: SET con TTL en una sola operación (directiva 4).
            // La clave es el UUID del jugador, garantizando una sesión activa por jugador.
            await _sessionCache.SetSessionAsync(player.Id.ToString(), sessionToken, _sessionTtl);

            return (player, sessionToken);
        }

        private static string GenerateSecureToken()
        {
            // RandomNumberGenerator es criptográficamente seguro y thread-safe.
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "IceLauncherSecretSalt");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
