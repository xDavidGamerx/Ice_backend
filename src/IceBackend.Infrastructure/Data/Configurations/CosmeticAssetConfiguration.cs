using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class CosmeticAssetConfiguration : IEntityTypeConfiguration<CosmeticAsset>
    {
        public void Configure(EntityTypeBuilder<CosmeticAsset> builder)
        {
            builder.ToTable("cosmetic_assets");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.Property(e => e.CosmeticType)
                   .HasConversion<string>()
                   .HasMaxLength(24);

            builder.Property(e => e.DisplayName)
                   .HasMaxLength(128)
                   .IsRequired();

            builder.Property(e => e.AssetVersion)
                   .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .IsRequired();

            // Las versiones binarias (CAS) se configuran en CosmeticAssetVersionConfiguration.
            // La relación inversa se declara aquí para completitud del grafo de navegación.
            builder.HasMany(e => e.Versions)
                   .WithOne(v => v.CosmeticAsset)
                   .HasForeignKey(v => v.CosmeticAssetId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
