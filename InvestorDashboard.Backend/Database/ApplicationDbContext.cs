using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorDashboard.Backend.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
      public string CurrentUserId { get; set; }
    public DbSet<ConfigurationItem> ConfigurationItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CryptoAddress> CryptoAddresses { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
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

            builder.Entity<Transaction>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<Transaction>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");
            builder.Entity<Transaction>()
                .HasIndex(x => x.Hash)
                .IsUnique();

            builder.Entity<CryptoAddress>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<CryptoAddress>()
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
