using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Services;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public override TimeSpan Period => TimeSpan.FromMinutes(1);

        public RefreshTransactionsJob(IEnumerable<ICryptoService> cryptoServices)
        {
            _cryptoServices = cryptoServices ?? throw new System.ArgumentNullException(nameof(cryptoServices));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            Parallel.ForEach(_cryptoServices, x => x.RefreshInboundTransactions().Wait());
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            await Out.WriteLineAsync($"Transaction refresh completed for currencies: { string.Join(", ", _cryptoServices.Select(x => x.Currency.ToString())) }");
        }
    }
}
