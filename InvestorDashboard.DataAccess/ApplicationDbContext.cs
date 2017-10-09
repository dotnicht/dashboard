using InvestorDashboard.DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace InvestorDashboard.DataAccess
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
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

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
