using System;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Domain.Services;
using Xunit;

namespace IceBackend.UnitTests
{
    public class PlayerSubscriptionTests
    {
        private readonly PlayerId _playerId;

        public PlayerSubscriptionTests()
        {
            _playerId = new PlayerId(Guid.NewGuid());
        }

        [Fact]
        public void AssignIcePlusSubscription_ShouldActivateSubscriptionAndRequireSync()
        {
            // Arrange
            var player = new Player(_playerId, "TestUser", UuidType.ICE, "some_hash");
            var currentTime = DateTime.UtcNow;

            // Act
            player.AssignIcePlusSubscription("sub_123", autoRenew: true, durationDays: 30, currentTime);

            // Assert
            Assert.NotNull(player.Subscription);
            Assert.True(player.Subscription.IsActive);
            Assert.Equal("sub_123", player.Subscription.StripeSubscriptionId);
            Assert.True(player.Subscription.AutoRenew);
            Assert.True(player.RequiresSessionSync);
            Assert.Equal(currentTime.AddDays(30), player.Subscription.ExpiresAt);
        }

        [Fact]
        public void IncrementIcePlusMonths_ShouldIncreaseAccumulatedMonths()
        {
            // Arrange
            var player = new Player(_playerId, "TestUser", UuidType.ICE, "some_hash");
            var currentTime = DateTime.UtcNow;

            // Act
            player.IncrementIcePlusMonths(currentTime);
            player.IncrementIcePlusMonths(currentTime);

            // Assert
            Assert.NotNull(player.Subscription);
            Assert.Equal(2, player.Subscription.AccumulatedMonths);
        }

        [Fact]
        public void CancelIcePlusSubscription_ShouldDeactivateSubscription()
        {
            // Arrange
            var player = new Player(_playerId, "TestUser", UuidType.ICE, "some_hash");
            var currentTime = DateTime.UtcNow;
            player.AssignIcePlusSubscription("sub_123", autoRenew: true, durationDays: 30, currentTime);

            // Act
            player.CancelIcePlusSubscription(currentTime);

            // Assert
            Assert.NotNull(player.Subscription);
            Assert.False(player.Subscription.IsActive);
            Assert.False(player.Subscription.AutoRenew);
            Assert.True(player.RequiresSessionSync);
        }

        [Theory]
        [InlineData(0, "pink-green")]
        [InlineData(1, "pink-green")]
        [InlineData(2, "blue")]
        [InlineData(3, "green")]
        [InlineData(4, "red")]
        [InlineData(5, "yellow")]
        [InlineData(6, "purple")]
        [InlineData(12, "gold")]
        [InlineData(15, "gold")] // Interpolación intermedia
        [InlineData(18, "diamond")]
        [InlineData(24, "emerald")]
        [InlineData(36, "rainbow")]
        [InlineData(40, "rainbow")]
        public void IcePlusBenefitsProvider_ShouldResolveIconColorBasedOnMonths(int months, string expectedColor)
        {
            // Act
            var benefits = IcePlusBenefitsProvider.GetBenefits(months);

            // Assert
            Assert.Equal(expectedColor, benefits.IconColor);
            Assert.True(benefits.HasClothCloaks);
            Assert.True(benefits.NoAds);
            Assert.True(benefits.UnlimitedFriends);
            Assert.Equal(10.0m, benefits.DiscountPercentage);
            Assert.Contains(Guid.Parse("00000000-0000-0000-0000-000000000001"), benefits.ExclusiveCosmeticIds);
        }
    }
}
