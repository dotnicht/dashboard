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
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IExchangeRateService _exchangeRateService;

        public RefreshExchangeRatesJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IOptions<TokenSettings> tokenSettings, IExchangeRateService exchangeRateService) 
            : base(loggerFactory, options)
        {
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var currencies = new[] { Currency.BTC, Currency.ETH };
            foreach (var currency in currencies)
            {
                try
                {
                    await _exchangeRateService.RefreshExchangeRate(currency, _tokenSettings.Value.Currency);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while refreshing {currency} exchange rate.");
                }
            }
        }
    }
}
