using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.ToTable("players");

            // Mapeo del Value Object PlayerId
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                   .HasConversion(id => id.Value, value => new PlayerId(value))
                   .HasColumnType("uuid");

            builder.HasIndex(e => e.Username).IsUnique();

            builder.Property(e => e.UuidType)
                   .HasConversion<string>()
                   .HasMaxLength(10);



            builder.Property(e => e.RequiresSessionSync)
                   .HasColumnName("requires_session_sync")
                   .HasColumnType("boolean")
                   .HasDefaultValue(false)
                   .IsRequired();

            // Mapeo de campos de respaldo privados de wearables (Nullable Integers para PostgreSQL Null Bitmap)
            ConfigureWearableField(builder, "_equippedHatInternalId", "equipped_hat_id");
            ConfigureWearableField(builder, "_equippedWingInternalId", "equipped_wing_id");
            ConfigureWearableField(builder, "_equippedCapeInternalId", "equipped_cape_id");
            ConfigureWearableField(builder, "_equippedShirtInternalId", "equipped_shirt_id");
            ConfigureWearableField(builder, "_equippedPantsInternalId", "equipped_pants_id");
            ConfigureWearableField(builder, "_equippedShoesInternalId", "equipped_shoes_id");

            // Mapeo del CosmeticId (UUID de dominio) expuesto públicamente
            builder.Property(e => e.EquippedHatId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_hat_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);

            builder.Property(e => e.EquippedWingId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_wing_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);

            builder.Property(e => e.EquippedCapeId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_cape_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);

            builder.Property(e => e.EquippedShirtId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_shirt_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);

            builder.Property(e => e.EquippedPantsId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_pants_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);

            builder.Property(e => e.EquippedShoesId)
                   .HasConversion(id => id != null ? id.Value : (Guid?)null, value => value != null ? new CosmeticId(value.Value) : null)
                   .HasColumnName("equipped_shoes_uuid")
                   .HasColumnType("uuid")
                   .IsRequired(false);
        }

        private void ConfigureWearableField(EntityTypeBuilder<Player> builder, string fieldName, string columnName)
        {
            builder.Property<int?>(fieldName)
                   .HasColumnName(columnName)
                   .HasColumnType("integer")
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .IsRequired(false);

            // Llave foránea privada contra el InternalId de CosmeticAsset
            builder.HasOne<CosmeticAsset>()
                   .WithMany()
                   .HasForeignKey(fieldName)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
