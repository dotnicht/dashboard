using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            var result = Get<List<decimal>>($"{_options.Value.ApiUri}ticker/t{baseCurrency}{quoteCurrency}");
            result.Wait();
            return result.Result?[0] ?? throw new InvalidOperationException("An error occurred while retrieving exchange rate.");
        }

        private async Task<T> Get<T>(string url) where T : new()
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var response = await client.Execute<T>(request).ConfigureAwait(false);
            return response.Data;
        }
    }
}
