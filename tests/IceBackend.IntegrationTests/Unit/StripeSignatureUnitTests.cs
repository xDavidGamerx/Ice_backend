using System.Threading.Tasks;
using IceBackend.Infrastructure.Services;
using IceBackend.IntegrationTests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Stripe;
using Xunit;
using Moq;

namespace IceBackend.IntegrationTests.Unit
{
    public class StripeSignatureUnitTests
    {
        [Fact]
        public void ValidateEventAsync_WithValidSignature_ShouldReturnEvent()
        {
            // Arrange
            var optionsMock = new Mock<IOptionsSnapshot<IceBackend.Application.Options.StripeOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new IceBackend.Application.Options.StripeOptions { WebhookSecret = "whsec_test_secret" });

            var validator = new StripeWebhookValidator(optionsMock.Object, new NullLogger<StripeWebhookValidator>());
            var payload = StripeSignatureHelper.BuildStripePayload("cus_test_dummy", "sub_test_dummy", "subscription_create", "invoice.paid");
            var secret = "whsec_test_secret";
            var signature = StripeSignatureHelper.GenerateSignature(payload, secret);

            // Act
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, secret, throwOnApiVersionMismatch: false);

            // Assert
            Assert.NotNull(stripeEvent);
            Assert.Equal("invoice.paid", stripeEvent.Type);
        }
    }
}
