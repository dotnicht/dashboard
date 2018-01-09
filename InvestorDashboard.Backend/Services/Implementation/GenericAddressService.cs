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

        public async Task CreateMissingAddresses()
        {
            foreach (var user in Context.Users.Include(x => x.CryptoAddresses).ToArray())
            {
                if (!user.CryptoAddresses.Any(x => x.Currency == Currency.DTT && x.Type == CryptoAddressType.Transfer))
                {
                    Context.CryptoAddresses.Add(new CryptoAddress
                    {
                        Currency = Currency.DTT,
                        UserId = user.Id,
                        Type = CryptoAddressType.Transfer
                    });

                    await Context.SaveChangesAsync();
                }

                foreach (var service in _cryptoServices)
                {
                    if (user.CryptoAddresses.Any(x => x.Currency == service.Settings.Value.Currency && x.Type == CryptoAddressType.Investment))
                    {
                        await service.CreateCryptoAddress(user.Id);
                    }
                }
            }
        }
    }
}
