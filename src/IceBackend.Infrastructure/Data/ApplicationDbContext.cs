using System.Text.Json;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IceBackend.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<ExternalAuth> PlayerExternalAuths { get; set; } = null!;
        public DbSet<BootstrapToken> BootstrapTokens { get; set; } = null!;
        public DbSet<CosmeticAsset> CosmeticAssets { get; set; } = null!;
        public DbSet<PlayerCosmetic> PlayerCosmetics { get; set; } = null!;
        public DbSet<PlayerCosmeticOwnership> PlayerCosmeticOwnerships { get; set; } = null!;
        public DbSet<PaymentEvent> PaymentEvents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==============================================================================
            // 1. IDENTITY SYSTEM
            // ==============================================================================
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("players");
                
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.PlayerUuid).IsUnique();

                entity.Property(e => e.UuidType)
                      .HasConversion<string>()
                      .HasMaxLength(10);
            });

            modelBuilder.Entity<ExternalAuth>(entity =>
            {
                entity.ToTable("player_external_auth");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Provider, e.ExternalId }).IsUnique();

                entity.Property(e => e.Provider)
                      .HasConversion<string>()
                      .HasMaxLength(20);

                entity.HasOne(e => e.Player)
                      .WithMany(p => p.ExternalAuths)
                      .HasForeignKey(e => e.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BootstrapToken>(entity =>
            {
                entity.ToTable("bootstrap_tokens");

                entity.HasKey(e => e.TokenHash);

                entity.HasOne(e => e.Player)
                      .WithMany(p => p.BootstrapTokens)
                      .HasForeignKey(e => e.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==============================================================================
            // 2. COSMETICS SYSTEM
            // ==============================================================================
            modelBuilder.Entity<CosmeticAsset>(entity =>
            {
                entity.ToTable("cosmetic_assets");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AssetKey).IsUnique();

                entity.Property(e => e.CosmeticType)
                      .HasConversion<string>()
                      .HasMaxLength(24);

                entity.Property(e => e.Metadata)
                      .HasColumnType("jsonb")
                      .HasConversion(
                          v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                          v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
                      );
            });

            modelBuilder.Entity<PlayerCosmetic>(entity =>
            {
                entity.ToTable("player_cosmetics");

                // PlayerId as both Primary Key and Foreign Key (One-to-One)
                entity.HasKey(e => e.PlayerId);

                entity.HasOne(e => e.Player)
                      .WithOne(p => p.Cosmetic)
                      .HasForeignKey<PlayerCosmetic>(e => e.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Hat).WithMany().HasForeignKey(e => e.HatId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Wing).WithMany().HasForeignKey(e => e.WingId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Cape).WithMany().HasForeignKey(e => e.CapeId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Shirt).WithMany().HasForeignKey(e => e.ShirtId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Pants).WithMany().HasForeignKey(e => e.PantsId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Shoes).WithMany().HasForeignKey(e => e.ShoesId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<PlayerCosmeticOwnership>(entity =>
            {
                entity.ToTable("player_cosmetic_ownership");

                entity.HasKey(e => new { e.PlayerId, e.CosmeticId });

                entity.HasOne(e => e.Player)
                      .WithMany(p => p.CosmeticOwnerships)
                      .HasForeignKey(e => e.PlayerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Cosmetic)
                      .WithMany(c => c.Ownerships)
                      .HasForeignKey(e => e.CosmeticId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==============================================================================
            // 3. BILLING & AUDIT
            // ==============================================================================
            modelBuilder.Entity<PaymentEvent>(entity =>
            {
                entity.ToTable("payment_events");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProviderEventId).IsUnique();

                entity.Property(e => e.Provider)
                      .HasConversion<string>()
                      .HasMaxLength(24);

                entity.Property(e => e.RawEvent)
                      .HasColumnType("jsonb");

                entity.HasOne(e => e.Player)
                      .WithMany(p => p.PaymentEvents)
                      .HasForeignKey(e => e.PlayerUuid)
                      .HasPrincipalKey(p => p.PlayerUuid)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
