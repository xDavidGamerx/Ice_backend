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

            // Clave primaria subrogada de base de datos (secuencial interno)
            builder.HasKey(e => e.InternalId);
            builder.Property(e => e.InternalId)
                   .HasColumnName("internal_id")
                   .ValueGeneratedOnAdd();

            // Clave de dominio no enumerable (UUID) expuesta al exterior
            builder.Property(e => e.Id)
                   .HasConversion(id => id.Value, value => new CosmeticId(value))
                   .HasColumnName("id")
                   .HasColumnType("uuid")
                   .IsRequired();

            builder.HasIndex(e => e.Id).IsUnique();

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

            // Relación con las versiones de assets apuntando al InternalId
            builder.HasMany(e => e.Versions)
                   .WithOne(v => v.CosmeticAsset)
                   .HasForeignKey(v => v.CosmeticAssetInternalId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
