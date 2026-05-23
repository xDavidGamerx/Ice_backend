using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class PlayerSubscriptionConfiguration : IEntityTypeConfiguration<PlayerSubscription>
    {
        public void Configure(EntityTypeBuilder<PlayerSubscription> builder)
        {
            builder.ToTable("player_subscriptions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.Property(e => e.PlayerId)
                   .HasConversion(id => id.Value, value => new PlayerId(value))
                   .HasColumnType("uuid");

            builder.Property(e => e.IsActive)
                   .HasColumnName("IsActive")
                   .HasColumnType("boolean")
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(e => e.ExpiresAt)
                   .HasColumnName("ExpiresAt")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired(false);

            builder.Property(e => e.StripeSubscriptionId)
                   .HasColumnName("StripeSubscriptionId")
                   .HasMaxLength(255)
                   .IsRequired(false);

            builder.Property(e => e.AutoRenew)
                   .HasColumnName("AutoRenew")
                   .HasColumnType("boolean")
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(e => e.AccumulatedMonths)
                   .HasColumnName("AccumulatedMonths")
                   .HasColumnType("integer")
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(e => e.UpdatedAt)
                   .HasColumnName("UpdatedAt")
                   .HasColumnType("timestamp with time zone")
                   .IsRequired();

            // Configurar relación 1-a-1 con Player
            builder.HasOne(e => e.Player)
                   .WithOne(p => p.Subscription)
                   .HasForeignKey<PlayerSubscription>(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
