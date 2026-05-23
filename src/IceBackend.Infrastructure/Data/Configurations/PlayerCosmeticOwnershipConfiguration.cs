using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class PlayerCosmeticOwnershipConfiguration : IEntityTypeConfiguration<PlayerCosmeticOwnership>
    {
        public void Configure(EntityTypeBuilder<PlayerCosmeticOwnership> builder)
        {
            builder.ToTable("player_cosmetic_ownership");

            builder.HasKey(e => new { e.PlayerId, e.CosmeticAssetInternalId });

            builder.Property(e => e.PlayerId)
                   .HasConversion(id => id.Value, value => new PlayerId(value))
                   .HasColumnType("uuid");

            builder.Property(e => e.CosmeticAssetInternalId)
                   .HasColumnName("cosmetic_asset_internal_id")
                   .HasColumnType("integer");

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.CosmeticOwnerships)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Cosmetic)
                   .WithMany(c => c.Ownerships)
                   .HasForeignKey(e => e.CosmeticAssetInternalId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
