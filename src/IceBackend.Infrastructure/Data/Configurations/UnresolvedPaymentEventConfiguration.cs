using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class UnresolvedPaymentEventConfiguration : IEntityTypeConfiguration<UnresolvedPaymentEvent>
    {
        public void Configure(EntityTypeBuilder<UnresolvedPaymentEvent> builder)
        {
            builder.ToTable("unresolved_payment_events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.HasIndex(e => e.ProviderEventId).IsUnique();

            builder.Property(e => e.Provider)
                   .HasConversion<string>()
                   .HasMaxLength(24);

            builder.Property(e => e.RawEvent)
                   .HasColumnType("jsonb");
        }
    }
}
