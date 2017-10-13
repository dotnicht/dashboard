using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ExchangeRateService : IExchangeRateService
    {
        private readonly IOptions<ExchangeRate> _options;

        public ExchangeRateService(IOptions<ExchangeRate> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency)
        {
            if (baseCurrency == quoteCurrency)
            {
                return 1;
            }
             
            if (baseCurrency == Currency.DTT || quoteCurrency == Currency.DTT)
            {
                if (baseCurrency == Currency.DTT && quoteCurrency == Currency.USD)
                {
                    return _options.Value.DTTUSD;
                }

                throw new NotSupportedException("DTT conversions currently not supported.");
            }

            var result = RestUtil.Get<List<decimal>>($"{_options.Value.ApiUri}ticker/t{baseCurrency}{quoteCurrency}");
            if (result != null && result.Count > 0)
            {
                return result[0];
            }

            throw new InvalidOperationException("");
        }
    }
}
