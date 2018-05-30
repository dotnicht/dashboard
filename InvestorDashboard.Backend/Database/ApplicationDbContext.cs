using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestorDashboard.Backend.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public const string DefaultConnectionStringName = "DefaultConnection";

        public DbSet<CryptoTransaction> CryptoTransactions { get; set; }
        public DbSet<CryptoAddress> CryptoAddresses { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<DashboardHistoryItem> DashboardHistoryItems { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public static void Initialize(IServiceCollection serviceCollection, IConfigurationRoot configuration)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            serviceCollection.AddDbContext<ApplicationDbContext>(x =>
            {
                x.UseSqlServer(configuration.GetConnectionString(DefaultConnectionStringName), y => y.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));
                x.UseOpenIddict();
            }, ServiceLifetime.Transient);

            serviceCollection.BuildServiceProvider().GetRequiredService<ApplicationDbContext>().Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseOpenIddict();

            builder.Entity<CryptoTransaction>()
                .HasIndex(x => new { x.Hash, x.Direction, x.ExternalId, x.CryptoAddressId })
                .IsUnique(true);

            builder.Entity<CryptoTransaction>()
                .HasIndex(x => x.ExternalId)
                .IsUnique();

            builder.Entity<CryptoAddress>()
                .HasIndex(x => new { x.Currency, x.Type, x.IsDisabled, x.UserId })
                .IsUnique();

            builder.Entity<DashboardHistoryItem>()
                .HasIndex(x => new { x.Created, x.Currency })
                .IsUnique();

            builder.Entity<ExchangeRate>()
                .HasIndex(x => x.Created)
                .IsUnique(false);

            builder.Entity<ApplicationUser>()
                .HasIndex(x => x.ReferralCode)
                .IsUnique(true);

            foreach (var property in GetProperties(builder, p => p.ClrType == typeof(decimal)))
            {
                property.Relational().ColumnType = "decimal(18, 6)";
            }

            foreach (var property in GetProperties(builder, p => p.ClrType == typeof(DateTime) && p.Name == "Created"))
            {
                property.Relational().DefaultValueSql = "GETUTCDATE()";
            }
        }

        private static IEnumerable<IMutableProperty> GetProperties(ModelBuilder builder, Func<IMutableProperty, bool> predicate)
        {
            return builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(predicate);
        }
    }
}
