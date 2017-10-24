using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshExchangeRatesJob : JobBase
    {
        private readonly IExchangeRateService _exchangeRateService;

        public override TimeSpan Period => TimeSpan.FromMinutes(1);

        public RefreshExchangeRatesJob(IExchangeRateService exchangeRateService)
        {
            _exchangeRateService = exchangeRateService ?? throw new System.ArgumentNullException(nameof(exchangeRateService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var currencies = new[] { Currency.BTC, Currency.ETH };
            Parallel.ForEach(currencies, async x =>
            {
                await _exchangeRateService.RefreshExchangeRate(x);
            });

            await Out.WriteLineAsync($"Exchange rates update completed for currencies: { string.Join(", ", currencies.Select(x => x.ToString())) }");
        }
    }
}
