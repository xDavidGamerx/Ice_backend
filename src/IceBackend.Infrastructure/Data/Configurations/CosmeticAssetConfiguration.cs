using System.Collections.Generic;
using System.Text.Json;
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

            builder.HasIndex(e => e.AssetKey).IsUnique();

            builder.Property(e => e.CosmeticType)
                   .HasConversion<string>()
                   .HasMaxLength(24);

            var comparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, object>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
            );

            builder.Property(e => e.Metadata)
                   .HasColumnType("jsonb")
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                       v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                   )
                   .Metadata.SetValueComparer(comparer);

            // Índice GIN nativo para consultas complejas sobre JSONB
            builder.HasIndex(e => e.Metadata)
                   .HasMethod("gin");
        }
    }
}
