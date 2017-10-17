

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Core;
using InvestorDashboard.Backend.Core.Interfaces;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace InvestorDashboard.Backend
{
    public interface IDatabaseInitializer
    {
        Task SeedAsync();
    }




    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly IAccountManager _accountManager;
        private readonly ILogger _logger;

        public DatabaseInitializer(ApplicationDbContext context, IAccountManager accountManager, ILogger<DatabaseInitializer> logger)
        {
            _accountManager = accountManager;
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await _context.Database.MigrateAsync().ConfigureAwait(false);

            if (!await _context.Users.AnyAsync())
            {
                _logger.LogInformation("Generating inbuilt accounts");

                const string adminRoleName = "administrator";
                const string userRoleName = "user";

                await ensureRoleAsync(adminRoleName, "Default administrator", ApplicationPermissions.GetAllPermissionValues());
                await ensureRoleAsync(userRoleName, "Default user", new string[] { });

                await createUserAsync("admin", "tempP@ss123", "Inbuilt Administrator", "admin@ebenmonney.com", "+1 (123) 000-0000", new string[] { adminRoleName });
                await createUserAsync("user", "tempP@ss123", "Inbuilt Standard User", "user@ebenmonney.com", "+1 (123) 000-0001", new string[] { userRoleName });

                _logger.LogInformation("Inbuilt account generation completed");
            }



        
        }



        private async Task ensureRoleAsync(string roleName, string description, string[] claims)
        {
            if ((await _accountManager.GetRoleByNameAsync(roleName)) == null)
            {
                ApplicationRole applicationRole = new ApplicationRole(roleName, description);

                var result = await this._accountManager.CreateRoleAsync(applicationRole, claims);

                if (!result.Item1)
                    throw new Exception($"Seeding \"{description}\" role failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");
            }
        }

        private async Task<ApplicationUser> createUserAsync(string userName, string password, string fullName, string email, string phoneNumber, string[] roles)
        {
            ApplicationUser applicationUser = new ApplicationUser
            {
                UserName = userName,
                FirstName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = true
            };

            var result = await _accountManager.CreateUserAsync(applicationUser, roles, password);

            if (!result.Item1)
                throw new Exception($"Seeding \"{userName}\" user failed. Errors: {string.Join(Environment.NewLine, result.Item2)}");


            return applicationUser;
        }
    }
}
