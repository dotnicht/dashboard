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

        public ExchangeRateService(ApplicationDbContext context, IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ExchangeRateSettings> options, IRestService restService)
            : base(context, serviceProvider, loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        public async Task<decimal> GetExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime? dateTime = null)
        {
            if (dateTime == null)
            {
                return await RefreshExchangeRate(baseCurrency, quoteCurrency);
            }

            using (var ctx = CreateContext())
            {
                if (!ctx.ExchangeRates.Any(x => x.Base == baseCurrency && x.Quote == quoteCurrency))
                {
                    await RefreshExchangeRate(baseCurrency, quoteCurrency, dateTime);
                }

                var ex = ctx.ExchangeRates
                    .Where(x => x.Base == baseCurrency && x.Quote == quoteCurrency)
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
                        throw new InvalidOperationException($"Couldn't find exchange rate for {baseCurrency}/{quoteCurrency} at {dateTime.Value}");
                    }

                    return rate.Rate;
                }
            }

            return await RefreshExchangeRate(baseCurrency, quoteCurrency, dateTime);
        }

        public async Task<decimal> RefreshExchangeRate(Currency baseCurrency, Currency quoteCurrency, DateTime? dateTime = null)
        {
            if (baseCurrency == quoteCurrency)
            {
                return 1;
            }

            using (var http = new HttpClient())
            {
                var ex = 0m;

                if (dateTime == null)
                {
                    var client = new PricesClient(http);
                    var response = await client.SingleAsync(baseCurrency.ToString(), new[] { quoteCurrency.ToString() });
                    ex = response.Values.Single();
                }
                else
                {
                    var adjusted = new DateTimeOffset(new DateTime(dateTime.Value.Year, dateTime.Value.Month, dateTime.Value.Day, dateTime.Value.Hour, 0, 0, DateTimeKind.Utc));

                    if (dateTime.Value.Minute > 30)
                    {
                        adjusted = adjusted.AddHours(1);
                    }

                    // TODO: adjust minute based ex rate.

                    var client = new HistoryClient(http);
                    var response = await client.HourAsync(baseCurrency.ToString(), quoteCurrency.ToString(), limit: 1, toDate: adjusted);
                    ex = response.Data.Single(x => x.Time == adjusted).Close;
                }

                using (var ctx = CreateContext())
                {
                    await ctx.ExchangeRates.AddAsync(new ExchangeRate { Base = baseCurrency, Quote = quoteCurrency, Rate = ex });
                    await ctx.SaveChangesAsync();
                }

                return ex;
            }
        }
    }
}
