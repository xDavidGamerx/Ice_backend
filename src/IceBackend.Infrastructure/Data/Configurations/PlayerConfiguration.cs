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

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.HasIndex(e => e.Username).IsUnique();

            builder.Property(e => e.UuidType)
                   .HasConversion<string>()
                   .HasMaxLength(10);
        }
    }
}
