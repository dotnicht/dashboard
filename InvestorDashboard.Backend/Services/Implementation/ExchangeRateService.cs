﻿using CryptoCompare;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ExchangeRateService : ContextService, IExchangeRateService
    {
        private readonly IOptions<ExchangeRateSettings> _options;
        private readonly IRestService _restService;

        public ExchangeRateService(ApplicationDbContext context, ILoggerFactory loggerFactory, IOptions<ExchangeRateSettings> options, IRestService restService)
            : base(context, loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        public async Task<decimal> GetExchangeRate(Currency currency, DateTime? dateTime = null)
        {
            if (!Context.ExchangeRates.Any(x => x.Base == currency))
            {
                await RefreshExchangeRate(currency);
            }

            var ex = Context.ExchangeRates.Where(x => x.Base == currency).OrderByDescending(x => x.Created);

            if (dateTime == null)
            {
                return ex.First().Rate;
            }

            var diff = ex
                .Where(x => x.Created >= dateTime.Value - _options.Value.LookupWindow && x.Created <= dateTime.Value + _options.Value.LookupWindow)
                .Min(x => Math.Abs((x.Created - dateTime.Value).Ticks));

            var rate = ex
                .Where(x => Math.Abs((x.Created - dateTime.Value).Ticks) == diff)
                .FirstOrDefault();

            if (rate == null)
            {
                throw new InvalidOperationException($"Couldn't find exchange rate for {currency} at {dateTime.Value}");
            }

            return rate.Rate;
        }

        public async Task RefreshExchangeRate(Currency currency)
        {
            using (var http = new HttpClient())
            {
                var client = new PricesClient(http);
                var response = await client.SingleAsync(currency.ToString(), new[] { "USD" });
                await Context.ExchangeRates.AddAsync(new ExchangeRate { Base = currency, Quote = Currency.USD, Rate = response.Values.Single() });
                await Context.SaveChangesAsync();
            }
        }
    }
}
