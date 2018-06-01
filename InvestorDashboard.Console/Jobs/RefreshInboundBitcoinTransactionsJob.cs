using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshInboundBitcoinTransactionsJob : JobBase
    {
        private readonly IBitcoinService _bitcoinService;

        public RefreshInboundBitcoinTransactionsJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IBitcoinService bitcoinService)
            : base(loggerFactory, options)
        {
            _bitcoinService = bitcoinService ?? throw new ArgumentNullException(nameof(bitcoinService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await (_bitcoinService.Settings.Value.UseDirectBlockAccess ? _bitcoinService.RefreshTransactionsByBalance() : _bitcoinService.RefreshInboundTransactions());
        }
    }
}
