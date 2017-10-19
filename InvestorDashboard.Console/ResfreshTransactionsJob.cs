using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using static System.Console;

namespace InvestorDashboard.Console
{
    public class ResfreshTransactionsJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var cryptoServices = Program.ServiceProvider.GetServices<ICryptoService>();
            Parallel.ForEach(cryptoServices, async x => await x.RefreshInboundTransactions());
            await Out.WriteLineAsync($"Transaction refresh completed for currencies: { string.Join(", ", cryptoServices.Select(x => x.Currency.ToString())) }");
        }
    }
}
