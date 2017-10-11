using InvestorDashboard.Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace InvestorDashboard.Backend
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WalletAddress> WalletAddresses { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.UseOpenIddict();

            builder.Entity<Transaction>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<Transaction>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<WalletAddress>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
            builder.Entity<WalletAddress>()
                .Property(b => b.Created)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
