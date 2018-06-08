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

        public GenericAddressService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IEnumerable<ICryptoService> cryptoServices) 
            : base(serviceProvider, loggerFactory)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        public async Task CreateMissingAddresses(string userId = null, bool includeInternal = true)
        {
            if (userId == null)
            {
                using (var ctx = CreateContext())
                {
                    var ids = ctx.Users
                        .Where(x => x.EmailConfirmed)
                        .Select(x => x.Id).ToArray();

                    foreach (var id in ids)
                    {
                        try
                        {
                            await CreateMissingAddressesInternal(id, includeInternal);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while creating missing addresses for user {id}.");
                        }
                    }
                }
            }
            else
            {
                await CreateMissingAddressesInternal(userId, includeInternal);
            }
        }

        public async Task<CryptoAddress> EnsureInternalAddress(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var ctx = CreateContext())
            {
                var address = ctx.CryptoAddresses.SingleOrDefault(x => x.Currency == Currency.Token && !x.IsDisabled && x.Type == CryptoAddressType.Internal && x.UserId == user.Id)
                    ?? ctx.CryptoAddresses.Add(new CryptoAddress { UserId = user.Id, Currency = Currency.Token, Type = CryptoAddressType.Internal }).Entity;
                await ctx.SaveChangesAsync();
                return address;
            }
        }

        private async Task CreateMissingAddressesInternal(string userId, bool includeInternal)
        {
            using (var ctx = CreateContext())
            {
                var user = ctx.Users.Include(x => x.CryptoAddresses).SingleOrDefault(x => x.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException($"User not found with ID {userId}.");
                }

                if (includeInternal && !user.CryptoAddresses.Any(x => x.Currency == Currency.Token && x.Type == CryptoAddressType.Internal && !x.IsDisabled))
                {
                    var address = new CryptoAddress
                    {
                        Currency = Currency.Token,
                        UserId = user.Id,
                        Type = CryptoAddressType.Internal
                    };

                    await ctx.CryptoAddresses.AddAsync(address);
                    await ctx.SaveChangesAsync();
                }

                var tasks = new List<Task<CryptoAddress>>();

                foreach (var service in _cryptoServices)
                {
                    if (!user.CryptoAddresses.Any(x => x.Currency == service.Settings.Value.Currency && x.Type == CryptoAddressType.Investment && !x.IsDisabled))
                    {
                        tasks.Add(service.CreateCryptoAddress(userId));
                    }
                }

                Task.WaitAll(tasks.ToArray());
            }
        }
    }
}
