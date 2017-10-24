using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Options;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public override TimeSpan Period => Options.Value.RefreshTransactionsPeriod;

        public RefreshTransactionsJob(ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices) 
            : base(context, options)
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
