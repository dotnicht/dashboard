using InvestorDashboard.Business.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;

namespace InvestorDashboard.Business.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<Bitcoin> _options;

        public BitcoinService(IOptions<Bitcoin> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
