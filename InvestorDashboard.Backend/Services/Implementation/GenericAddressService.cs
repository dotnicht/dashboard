using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class GenericAddressService : ContextService, IGenericAddressService
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public GenericAddressService(ApplicationDbContext context, ILoggerFactory loggerFactory, IEnumerable<ICryptoService> cryptoServices) : base(context, loggerFactory)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        public async Task CreateMissingAddresses(string userId = null)
        {
            if (userId == null)
            {
                foreach (var id in Context.Users.Select(x => x.Id).ToArray())
                {
                    try
                    {
                        await CreateMissingAddressesInternal(id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while refreshing balance for user {id}.");
                    }
                }
            }
            else
            {
                await CreateMissingAddressesInternal(userId);
            }
        }

        private async Task CreateMissingAddressesInternal(string userId)
        {
            var user = Context.Users.Include(x => x.CryptoAddresses).SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
            }

            if (!user.CryptoAddresses.Any(x => x.Currency == Currency.Token && x.Type == CryptoAddressType.Transfer && !x.IsDisabled))
            {
                var address = new CryptoAddress
                {
                    Currency = Currency.Token,
                    UserId = user.Id,
                    Type = CryptoAddressType.Transfer
                };

                await Context.CryptoAddresses.AddAsync(address);
                await Context.SaveChangesAsync();
            }

            foreach (var service in _cryptoServices)
            {
                if (!user.CryptoAddresses.Any(x => x.Currency == service.Settings.Value.Currency && x.Type == CryptoAddressType.Investment && !x.IsDisabled))
                {
                    await service.CreateCryptoAddress(user.Id);
                }
            }
        }
    }
}
