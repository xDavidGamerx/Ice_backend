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

            builder.HasKey(e => new { e.PlayerId, e.CosmeticId });

            builder.Property(e => e.PlayerId)
                   .HasColumnType("uuid");

            builder.Property(e => e.CosmeticId)
                   .HasColumnType("uuid");

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.CosmeticOwnerships)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Cosmetic)
                   .WithMany(c => c.Ownerships)
                   .HasForeignKey(e => e.CosmeticId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
