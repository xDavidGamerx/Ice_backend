using System;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/v1/dev")]
    public class DevSeedController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHostEnvironment _env;

        public DevSeedController(ApplicationDbContext dbContext, IHostEnvironment env)
        {
            _dbContext = dbContext;
            _env = env;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> Seed()
        {
            // ── GUARDIA DE ENTORNO: 404 fuera de Development ──
            if (!_env.IsDevelopment())
                return NotFound();

            // ── IDEMPOTENCIA: no sobrescribir datos existentes ──
            if (await _dbContext.Players.AnyAsync())
                return Conflict(new { message = "Base de datos ya sembrada. Omite este endpoint si ya tienes datos." });

            // ── DATOS DETERMINISTAS ──
            var player1Id = new PlayerId(Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var player2Id = new PlayerId(Guid.Parse("00000000-0000-0000-0000-000000000002"));

            var cosmetic1 = new CosmeticAsset(
                new CosmeticId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
                CosmeticType.CAPE, "Capa Legendaria");
            var cosmetic2 = new CosmeticAsset(
                new CosmeticId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")),
                CosmeticType.WING, "Alas Etereas");
            var cosmetic3 = new CosmeticAsset(
                new CosmeticId(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc")),
                CosmeticType.HAT, "Halo Radiante");

            var player1 = new Player(player1Id, "dev_demo_player1", UuidType.ICE, null);
            var player2 = new Player(player2Id, "dev_demo_player2", UuidType.PREMIUM, null);

            // ── TRANSACCION ──
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            _dbContext.CosmeticAssets.AddRange(cosmetic1, cosmetic2, cosmetic3);
            _dbContext.Players.AddRange(player1, player2);
            await _dbContext.SaveChangesAsync(); // Materializa InternalId de cosmeticos

            // Ownerships usando InternalId materializados (sin riesgo de re-insert)
            var ownership1 = new PlayerCosmeticOwnership(player1Id, cosmetic1.InternalId, "seed_demo");
            var ownership2 = new PlayerCosmeticOwnership(player1Id, cosmetic2.InternalId, "seed_demo");
            _dbContext.PlayerCosmeticOwnerships.AddRange(ownership1, ownership2);

            // Suscripcion ICE+ activa con 3 meses acumulados
            player1.AssignIcePlusSubscription("seed_demo", false, 365, DateTime.UtcNow);
            player1.IncrementIcePlusMonths(DateTime.UtcNow);
            player1.IncrementIcePlusMonths(DateTime.UtcNow);
            player1.IncrementIcePlusMonths(DateTime.UtcNow);
            // → AccumulatedMonths = 3

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                playersSeeded = 2,
                cosmeticsSeeded = 3,
                subscriptionsSeeded = 1,
                ownershipsSeeded = 2
            });
        }
    }
}
