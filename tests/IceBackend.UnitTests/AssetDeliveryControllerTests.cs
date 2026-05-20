using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IceBackend.Api.Controllers;
using IceBackend.Application.Interfaces;
using IceBackend.Domain.Entities;
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
        public async Task RequestDelivery_WhenUserDoesNotOwnCosmetic_Returns403Forbidden()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var playerId = Guid.NewGuid();
            var cosmeticId = Guid.NewGuid();
            var assetHash = "fake-sha256-hash";

            var mockSessionCache = new Mock<ISessionCache>();
            mockSessionCache.Setup(s => s.GetCosmeticIdByHashAsync(assetHash))
                .ReturnsAsync(cosmeticId);
            mockSessionCache.Setup(s => s.IsCosmeticOwnedAsync(playerId, cosmeticId))
                .ReturnsAsync(false);

            var mockCdnSigner = new Mock<ICdnUrlSigner>();
            var mockAssetTokenService = new Mock<IAssetTokenService>();

            var controller = new AssetDeliveryController(
                mockSessionCache.Object, 
                dbContext, 
                mockCdnSigner.Object, 
                mockAssetTokenService.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, playerId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext { User = principal };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.RequestDelivery(new AssetDeliveryController.RequestDeliveryBody { Hash = assetHash });

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task RequestDelivery_WhenUserOwnsCosmetic_ReturnsDeliveryToken()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var playerId = Guid.NewGuid();
            var cosmeticId = Guid.NewGuid();
            var assetHash = "fake-sha256-hash";
            var expectedToken = "signed.delivery.token";

            var mockSessionCache = new Mock<ISessionCache>();
            mockSessionCache.Setup(s => s.GetCosmeticIdByHashAsync(assetHash))
                .ReturnsAsync(cosmeticId);
            mockSessionCache.Setup(s => s.IsCosmeticOwnedAsync(playerId, cosmeticId))
                .ReturnsAsync(true);

            var mockCdnSigner = new Mock<ICdnUrlSigner>();
            
            var mockAssetTokenService = new Mock<IAssetTokenService>();
            mockAssetTokenService.Setup(s => s.GenerateDeliveryToken(playerId, assetHash, It.IsAny<TimeSpan>()))
                .Returns(expectedToken);

            var controller = new AssetDeliveryController(
                mockSessionCache.Object, 
                dbContext, 
                mockCdnSigner.Object, 
                mockAssetTokenService.Object);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, playerId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            
            var httpContext = new DefaultHttpContext { User = principal };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await controller.RequestDelivery(new AssetDeliveryController.RequestDeliveryBody { Hash = assetHash });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dynamicData = okResult.Value;
            Assert.NotNull(dynamicData);
            
            var tokenProp = dynamicData.GetType().GetProperty("token")?.GetValue(dynamicData, null) as string;
            var urlProp = dynamicData.GetType().GetProperty("url")?.GetValue(dynamicData, null) as string;

            Assert.Equal(expectedToken, tokenProp);
            Assert.Contains(expectedToken, urlProp);
        }

        [Fact]
        public void Deliver_WithInvalidToken_Returns403Forbidden()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var assetHash = "fake-sha256-hash";
            var invalidToken = "invalid-token";

            var mockSessionCache = new Mock<ISessionCache>();
            var mockCdnSigner = new Mock<ICdnUrlSigner>();
            
            var mockAssetTokenService = new Mock<IAssetTokenService>();
            Guid tempOut;
            mockAssetTokenService.Setup(s => s.ValidateDeliveryToken(invalidToken, assetHash, out tempOut))
                .Returns(false);

            var controller = new AssetDeliveryController(
                mockSessionCache.Object, 
                dbContext, 
                mockCdnSigner.Object, 
                mockAssetTokenService.Object);

            // Act
            var result = controller.Deliver(assetHash, invalidToken);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void Deliver_WithValidToken_Returns307RedirectToCdn()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var playerId = Guid.NewGuid();
            var assetHash = "fake-sha256-hash";
            var validToken = "valid-token";
            var expectedCdnUrl = "https://cdn.example.com/assets/fake-sha256-hash?signature=xyz";

            var mockSessionCache = new Mock<ISessionCache>();
            
            var mockCdnSigner = new Mock<ICdnUrlSigner>();
            mockCdnSigner.Setup(s => s.GeneratePresignedUrl(assetHash))
                .Returns((expectedCdnUrl, DateTime.UtcNow.AddMinutes(1)));

            var mockAssetTokenService = new Mock<IAssetTokenService>();
            Guid tempOut = playerId;
            mockAssetTokenService.Setup(s => s.ValidateDeliveryToken(validToken, assetHash, out tempOut))
                .Returns(true);

            var controller = new AssetDeliveryController(
                mockSessionCache.Object, 
                dbContext, 
                mockCdnSigner.Object, 
                mockAssetTokenService.Object);

            // Act
            var result = controller.Deliver(assetHash, validToken);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Equal(expectedCdnUrl, redirectResult.Url);
        }
    }
}
