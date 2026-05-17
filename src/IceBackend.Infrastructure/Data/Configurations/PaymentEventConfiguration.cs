using IceBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IceBackend.Infrastructure.Data.Configurations
{
    public class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
    {
        public void Configure(EntityTypeBuilder<PaymentEvent> builder)
        {
            builder.ToTable("payment_events");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .HasColumnType("uuid");

            builder.Property(e => e.PlayerId)
                   .HasColumnType("uuid");

            builder.HasIndex(e => e.ProviderEventId).IsUnique();

            builder.Property(e => e.Provider)
                   .HasConversion<string>()
                   .HasMaxLength(24);

            builder.Property(e => e.RawEvent)
                   .HasColumnType("jsonb");

            builder.HasOne(e => e.Player)
                   .WithMany(p => p.PaymentEvents)
                   .HasForeignKey(e => e.PlayerId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
