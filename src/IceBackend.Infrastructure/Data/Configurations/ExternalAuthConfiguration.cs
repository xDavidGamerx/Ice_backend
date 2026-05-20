using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class ExternalAuthConfiguration : IEntityTypeConfiguration<ExternalAuth>
    {
        public void Configure(EntityTypeBuilder<ExternalAuth> builder)
        {
            builder.ToTable("player_external_auth");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.Property(e => e.PlayerId)
                   .HasColumnType("uuid");

            builder.HasIndex(e => new { e.Provider, e.ExternalId }).IsUnique();

            builder.Property(e => e.Provider)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.ExternalAuths)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
