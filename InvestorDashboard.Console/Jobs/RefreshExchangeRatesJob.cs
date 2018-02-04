using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshExchangeRatesJob : JobBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public override TimeSpan Period => Options.Value.RefreshExchangeRatesPeriod;

        public RefreshExchangeRatesJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IExchangeRateService exchangeRateService) 
            : base(loggerFactory, context, options)
        {
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var currencies = new[] { Currency.BTC, Currency.ETH };
            foreach (var currency in currencies)
            {
                try
                {
                    await _exchangeRateService.RefreshExchangeRate(currency);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while refreshing {currency} exchange rate.");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _exchangeRateService.Dispose();
            base.Dispose(disposing);
        }
    }
}
