using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ExchangeRateService : IExchangeRateService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<ExchangeRateSettings> _options;
        private readonly ILogger _logger;

        public ExchangeRateService(ApplicationDbContext context, IOptions<ExchangeRateSettings> options, ILogger<ExchangeRateService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime? dateTime = null)
        {
            if (baseCurrency == quoteCurrency)
            {
                return 1;
            }

            return dateTime == null
                ? GetExchangeRateFromApi(baseCurrency, quoteCurrency)
                : GetExchangeRateFromDatabase(baseCurrency, quoteCurrency, dateTime.Value)
                    ?? throw CreateDbException(baseCurrency, quoteCurrency, dateTime.Value);
        }

        public decimal GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime dateTime, bool fallbacktoCurrent)
        {
            var result = GetExchangeRateFromDatabase(baseCurrency, quoteCurrency, dateTime);
            if (result == null)
            {
                var ex = CreateDbException(baseCurrency, quoteCurrency, dateTime);

                if (fallbacktoCurrent)
                {
                    _logger.LogInformation(ex.Message);
                    return GetExchangeRateFromApi(baseCurrency, quoteCurrency);
                }

                throw ex;
            }

            return result.Value;
        }

        public void RefreshExchangeRate(Currency baseCurrency)
        {
            _context.ExchangeRates.Add(new ExchangeRate { Base = baseCurrency, Quote = Currency.USD, Rate = GetExchangeRate(baseCurrency, Currency.USD) });
            _context.SaveChanges();
        }

        private decimal GetExchangeRateFromApi(Currency baseCurrency, Currency quoteCurrency)
        {
            if (baseCurrency == Currency.DTT || quoteCurrency == Currency.DTT)
            {
                if (baseCurrency == Currency.DTT && quoteCurrency == Currency.USD)
                {
                    return _options.Value.DTTUSD;
                }

                throw new NotSupportedException("DTT conversions currently not supported.");
            }

            var result = RestUtil.Get<List<decimal>>($"{_options.Value.ApiUri}ticker/t{baseCurrency}{quoteCurrency}");
            if (result?.Count > 0)
            {
                return result[0];
            }

            throw new InvalidOperationException("An error occurred while retrieving exchange rate from bitfinex.com.");
        }

        private decimal? GetExchangeRateFromDatabase(Currency baseCurrency, Currency quoteCurrency, DateTime dateTime) => 
            _context.ExchangeRates.OrderByDescending(x => x.Created).FirstOrDefault(x => x.Base == baseCurrency && x.Quote == quoteCurrency && x.Created <= dateTime)?.Rate;

        private InvalidOperationException CreateDbException(Currency baseCurrency, Currency quoteCurrency, DateTime dateTime) => 
            new InvalidOperationException($"Exchange rate record not found for currency pair {baseCurrency}/{quoteCurrency} and date & time {dateTime}.");
    }
}
