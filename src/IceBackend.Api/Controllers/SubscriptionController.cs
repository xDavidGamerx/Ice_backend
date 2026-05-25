using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/subscription")]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISessionCache _sessionCache;

        public SubscriptionController(ApplicationDbContext dbContext, ISessionCache sessionCache)
        {
            _dbContext = dbContext;
            _sessionCache = sessionCache;
        }

        /// <summary>
        /// Obtiene el estado actual de la suscripción del jugador autenticado y sus beneficios activos.
        /// </summary>
        [HttpGet("me")]
        public async Task<IActionResult> GetMySubscription()
        {
            var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (playerIdClaim == null || !Guid.TryParse(playerIdClaim.Value, out var playerId))
                return Unauthorized();

            var subscription = await _dbContext.Players
                .Where(p => p.Id == new PlayerId(playerId))
                .Select(p => p.Subscription)
                .FirstOrDefaultAsync();

            if (subscription == null)
            {
                return Ok(new
                {
                    isActive = false,
                    accumulatedMonths = 0,
                    expiresAt = (DateTime?)null,
                    stripeSubscriptionId = (string?)null,
                    autoRenew = false,
                    benefits = (object?)null
                });
            }

            var benefits = await _sessionCache.GetIcePlusBenefitsAsync(playerId);

            return Ok(new
            {
                isActive = subscription.IsActive,
                accumulatedMonths = subscription.AccumulatedMonths,
                expiresAt = subscription.ExpiresAt,
                stripeSubscriptionId = subscription.StripeSubscriptionId,
                autoRenew = subscription.AutoRenew,
                benefits = subscription.IsActive ? benefits : null
            });
        }
    }
}
