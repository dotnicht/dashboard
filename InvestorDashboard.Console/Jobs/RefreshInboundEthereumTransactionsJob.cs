using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshInboundEthereumTransactionsJob : JobBase
    {
        private readonly IEthereumService _ethereumService;

        public RefreshInboundEthereumTransactionsJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IEthereumService ethereumService)
            : base(loggerFactory, options)
        {
            _ethereumService = ethereumService ?? throw new ArgumentNullException(nameof(ethereumService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await (_ethereumService.Settings.Value.UseDirectBlockAccess ? _ethereumService.RefreshTransactionsByBalance() : _ethereumService.RefreshInboundTransactions());
        }
    }
}
