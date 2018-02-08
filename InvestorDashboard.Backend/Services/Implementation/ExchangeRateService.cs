using CryptoCompare;
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

            var ex = Context.ExchangeRates
                .Where(x => x.Base == currency)
                .OrderByDescending(x => x.Created);

            if (dateTime == null)
            {
                return ex.First().Rate;
            }

            var lower = dateTime.Value - _options.Value.LookupWindow;
            var upper = dateTime.Value + _options.Value.LookupWindow;

            var filtered = ex
                .Where(x => x.Created >= lower && x.Created <= upper)
                .ToArray();

            if (filtered.Any())
            {
                var diff = filtered.Min(x => Math.Abs((x.Created - dateTime.Value).Ticks));

                var rate = filtered
                    .Where(x => Math.Abs((x.Created - dateTime.Value).Ticks) == diff)
                    .FirstOrDefault();

                if (rate == null)
                {
                    throw new InvalidOperationException($"Couldn't find exchange rate for {currency} at {dateTime.Value}");
                }

                return rate.Rate;
            }

            return await RefreshExchangeRate(currency, dateTime);
        }

        public async Task<decimal> RefreshExchangeRate(Currency currency, DateTime? dateTime = null)
        {
            using (var http = new HttpClient())
            {
                decimal ex;

                if (dateTime == null)
                {
                    var client = new PricesClient(http);
                    var response = await client.SingleAsync(currency.ToString(), new[] { Currency.USD.ToString() });
                    ex = response.Values.Single();
                }
                else
                {
                    var adjusted = new DateTimeOffset(new DateTime(dateTime.Value.Year, dateTime.Value.Month, dateTime.Value.Day, dateTime.Value.Hour, 0, 0, DateTimeKind.Utc));
                    var client = new HistoryClient(http);
                    var response = await client.HourAsync(currency.ToString(), Currency.USD.ToString(), limit: 1, toDate: adjusted);
                    ex = response.Data.Single(x => x.Time == adjusted).Close;
                }

                await Context.ExchangeRates.AddAsync(new ExchangeRate { Base = currency, Quote = Currency.USD, Rate = ex });
                await Context.SaveChangesAsync();
                return ex;
            }
        }
    }
}
