using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Options;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshExchangeRatesJob : JobBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public override TimeSpan Period => Options.Value.RefreshExchangeRatesPeriod;

        public RefreshExchangeRatesJob(ApplicationDbContext context, IOptions<JobsSettings> options, IExchangeRateService exchangeRateService) 
            : base(context, options)
        {
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var currencies = new[] { Currency.BTC, Currency.ETH };
            foreach (var currency in currencies)
            {
                await _exchangeRateService.RefreshExchangeRate(currency);
            }

            await Out.WriteLineAsync($"Exchange rates update completed for currencies: { string.Join(", ", currencies.Select(x => x.ToString())) }");
        }
    }
}
