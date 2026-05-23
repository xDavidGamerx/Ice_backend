using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class BootstrapTokenConfiguration : IEntityTypeConfiguration<BootstrapToken>
    {
        public void Configure(EntityTypeBuilder<BootstrapToken> builder)
        {
            builder.ToTable("bootstrap_tokens");

            builder.HasKey(e => e.TokenHash);

            builder.Property(e => e.PlayerId)
                   .HasConversion(id => id.Value, value => new PlayerId(value))
                   .HasColumnType("uuid");

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.BootstrapTokens)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
