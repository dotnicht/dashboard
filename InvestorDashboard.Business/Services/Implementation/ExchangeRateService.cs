using InvestorDashboard.Business.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;

namespace InvestorDashboard.Business.Services.Implementation
{
    internal class ExchangeRateService : IExchangeRateService
    {
        private readonly IOptions<ExchangeRate> _options;

        public ExchangeRateService(IOptions<ExchangeRate> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
