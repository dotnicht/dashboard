using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace InvestorDashboard.Console
{
    public class RefreshExchangeRatesJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var exchangeRateService = Program.ServiceProvider.GetRequiredService<IExchangeRateService>();
            var currencies = new[] { Currency.BTC, Currency.ETH };
            Parallel.ForEach(currencies, async x => await exchangeRateService.RefreshExchangeRate(x));
            await Out.WriteLineAsync($"Exchange rates update completed for currencies: { string.Join(", ", currencies.Select(x => x.ToString())) }");
        }
    }
}
