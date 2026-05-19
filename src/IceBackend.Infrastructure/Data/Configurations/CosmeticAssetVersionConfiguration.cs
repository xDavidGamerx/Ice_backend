using System.Text.Json;
using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class CosmeticAssetVersionConfiguration : IEntityTypeConfiguration<CosmeticAssetVersion>
    {
        public void Configure(EntityTypeBuilder<CosmeticAssetVersion> builder)
        {
            builder.ToTable("cosmetic_asset_versions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.Property(e => e.CosmeticAssetId)
                   .HasColumnType("uuid");

            // ── Arquitectura almacenada como string (legible en BD) ──────────────
            builder.Property(e => e.Architecture)
                   .HasConversion<string>()
                   .HasMaxLength(16);

            // ── SHA-256: 64 chars hex, índice único por (cosmético + arquitectura).
            // Un cosmético no puede tener dos versiones de la misma arquitectura activas.
            builder.Property(e => e.Sha256Hash)
                   .HasMaxLength(64)
                   .IsRequired();

            builder.HasIndex(e => e.Sha256Hash);

            // Unicidad compuesta: un solo binario por (cosmético, arquitectura).
            builder.HasIndex(e => new { e.CosmeticAssetId, e.Architecture })
                   .IsUnique();

            builder.Property(e => e.SizeBytes)
                   .IsRequired();

            // ── MetadataJson → JSONB con índice GIN para queries sobre el contenido ──
            var jsonComparer = new ValueComparer<Dictionary<string, object>>(
                (c1, c2) => JsonSerializer.Serialize(c1, (JsonSerializerOptions?)null)
                         == JsonSerializer.Serialize(c2, (JsonSerializerOptions?)null),
                c => c == null ? 0 : JsonSerializer.Serialize(c, (JsonSerializerOptions?)null).GetHashCode(),
                c => JsonSerializer.Deserialize<Dictionary<string, object>>(
                         JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                         (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
            );

            builder.Property(e => e.MetadataJson)
                   .HasColumnType("jsonb")
                   .HasConversion(
                       v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                       v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null)
                            ?? new Dictionary<string, object>()
                   )
                   .Metadata.SetValueComparer(jsonComparer);

            builder.HasIndex(e => e.MetadataJson)
                   .HasMethod("gin");

            builder.Property(e => e.UploadedAt)
                   .IsRequired();

            // ── FK → CosmeticAsset ───────────────────────────────────────────────
            builder.HasOne(e => e.CosmeticAsset)
                   .WithMany(a => a.Versions)
                   .HasForeignKey(e => e.CosmeticAssetId)
                   .OnDelete(DeleteBehavior.Cascade); // Si se elimina el cosmético padre, se purgan sus versiones.
        }
    }
}
