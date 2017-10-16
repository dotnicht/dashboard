using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;

        public BitcoinService(IOptions<BitcoinSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
