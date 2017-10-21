using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;
        private readonly IKeyVaultService _keyVaultService;

        public Currency Currency => Currency.BTC;

        public BitcoinService(IOptions<BitcoinSettings> options, IKeyVaultService keyVaultService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        }

        public async Task UpdateUserDetails(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
        }

        public async Task RefreshInboundTransactions()
        {
        }

        public void Dispose()
        {
            //_context.Dispose();
        }
    }
}
