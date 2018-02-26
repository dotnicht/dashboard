using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestorDashboard.Backend.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public string CurrentUserId { get; set; }

        public DbSet<CryptoTransaction> CryptoTransactions { get; set; }
        public DbSet<CryptoAddress> CryptoAddresses { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<DashboardHistoryItem> DashboardHistoryItems { get; set; }
        public DbSet<EthereumBlock> EthereumBlocks { get; set; }
        public DbSet<EthereumTransaction> EthereumTransactions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseOpenIddict();

            builder.Entity<CryptoTransaction>()
                .HasIndex(x => x.Hash)
                .IsUnique();

            builder.Entity<CryptoTransaction>()
                .HasIndex(x => x.ExternalId)
                .IsUnique();

            builder.Entity<CryptoAddress>()
                .HasIndex(x => x.Address)
                .IsUnique();

            builder.Entity<DashboardHistoryItem>()
                .HasIndex(x => new { x.Created, x.Currency })
                .IsUnique();

            builder.Entity<ExchangeRate>()
                .HasIndex(x => x.Created)
                .IsUnique(false);

            builder.Entity<EthereumBlock>()
                .HasIndex(x => x.BlockHash)
                .IsUnique(true);

            builder.Entity<EthereumBlock>()
                .HasIndex(x => x.BlockIndex)
                .IsUnique(true);

            builder.Entity<EthereumTransaction>()
                .HasIndex(x => x.TransactionHash)
                .IsUnique(true);

            builder.Entity<EthereumTransaction>()
                .HasIndex(x => x.From)
                .IsUnique(false);

            builder.Entity<EthereumTransaction>()
                .HasIndex(x => x.To)
                .IsUnique(false);

            foreach (var property in GetProperties(builder, p => p.ClrType == typeof(decimal)))
            {
                property.Relational().ColumnType = "decimal(18, 6)";
            }

            foreach (var property in GetProperties(builder, p => p.ClrType == typeof(DateTime) && p.Name == "Created"))
            {
                property.Relational().DefaultValueSql = "GETUTCDATE()";
            }
        }

        private IEnumerable<IMutableProperty> GetProperties(ModelBuilder builder, Func<IMutableProperty, bool> predicate)
        {
            return builder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(predicate);
        }
    }
}
