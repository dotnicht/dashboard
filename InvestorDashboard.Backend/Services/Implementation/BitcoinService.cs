using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;

        public BitcoinService(IOptions<BitcoinSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task RefreshInboundTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
