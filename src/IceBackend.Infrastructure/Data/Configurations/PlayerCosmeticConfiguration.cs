using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class PlayerCosmeticConfiguration : IEntityTypeConfiguration<PlayerCosmetic>
    {
        public void Configure(EntityTypeBuilder<PlayerCosmetic> builder)
        {
            builder.ToTable("player_cosmetics");

            // Composite PK: (PlayerId, Slot) — garantiza una ranura única por jugador
            builder.HasKey(e => new { e.PlayerId, e.Slot });

            builder.Property(e => e.PlayerId)
                   .HasColumnType("uuid");

            builder.Property(e => e.Slot)
                   .HasConversion<string>()
                   .HasMaxLength(24);

            builder.Property(e => e.CosmeticId)
                   .HasColumnType("uuid");

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.EquippedCosmetics)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Cosmetic)
                   .WithMany()
                   .HasForeignKey(e => e.CosmeticId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
