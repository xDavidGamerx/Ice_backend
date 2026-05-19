using System;
using System.Threading.Tasks;
using IceBackend.Api.Controllers;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace IceBackend.UnitTests
{
    public class AssetDeliveryControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCosmeticAsset_WhenUserDoesNotOwnCosmetic_Returns403Forbidden()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            
            var playerId = Guid.NewGuid();
            var cosmeticId = Guid.NewGuid();
            var sessionToken = "valid-token-123";

            // Insert cosmetic in DB but NO ownership for this player
            dbContext.CosmeticAssets.Add(new CosmeticAsset
            {
                Id = cosmeticId,
                CosmeticType = CosmeticType.HAT,
                DisplayName = "Premium Hat",
                AssetVersion = 1,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            // Mock ISessionCache to return our playerId for the token
            var mockSessionCache = new Mock<ISessionCache>();
            mockSessionCache.Setup(s => s.GetPlayerIdBySessionAsync(sessionToken))
                .ReturnsAsync(playerId.ToString());

            // Mock ICdnUrlSigner (not strictly needed for this test since it should fail before signing, but good practice)
            var mockCdnSigner = new Mock<ICdnUrlSigner>();

            var controller = new AssetDeliveryController(mockSessionCache.Object, dbContext, mockCdnSigner.Object);
            
            // Set up HTTP Context with Authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {sessionToken}";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await controller.GetCosmeticAsset(cosmeticId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }
    }
}
