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
            var result = Get<List<decimal>>($"{_options.Value.ApiUri}ticker/t{baseCurrency}{quoteCurrency}");
            result.Wait();
            return result.Result[0];
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
