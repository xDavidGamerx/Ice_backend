using IceBackend.Domain.Entities;
using IceBackend.Infrastructure.Data.Configurations;
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
        public DbSet<CosmeticAssetVersion> CosmeticAssetVersions { get; set; } = null!;
        public DbSet<PlayerCosmeticOwnership> PlayerCosmeticOwnerships { get; set; } = null!;
        public DbSet<PaymentEvent> PaymentEvents { get; set; } = null!;
        public DbSet<UnresolvedPaymentEvent> UnresolvedPaymentEvents { get; set; } = null!;
        public DbSet<PlayerSubscription> PlayerSubscriptions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Todas las configuraciones viven en clases IEntityTypeConfiguration<T>
            // ubicadas en IceBackend.Infrastructure.Data.Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlayerConfiguration).Assembly);
        }
    }
}
