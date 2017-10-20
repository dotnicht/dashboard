using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvestorDashboard.Backend.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public string CurrentUserId { get; set; }

        public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
        public DbSet<CryptoTransaction> CryptoTransactions { get; set; }
        public DbSet<CryptoAddress> CryptoAddresses { get; set; }
        public DbSet<CryptoAccount> CryptoAccounts { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseOpenIddict();

            builder.Entity<ConfigurationItem>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<ConfigurationItem>()
                .HasIndex(x => x.Name)
                .IsUnique();

            builder.Entity<CryptoTransaction>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<CryptoTransaction>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<CryptoTransaction>()
                .HasIndex(x => x.Hash)
                .IsUnique();

            builder.Entity<CryptoAddress>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<CryptoAddress>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<CryptoAccount>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<CryptoAccount>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<ExchangeRate>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<ExchangeRate>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
