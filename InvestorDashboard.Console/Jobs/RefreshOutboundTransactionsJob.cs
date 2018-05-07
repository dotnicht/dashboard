using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshOutboundTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly ISmartContractService _smartContractService;

        public RefreshOutboundTransactionsJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, ISmartContractService smartContractService)
            : base(loggerFactory, options)
        {
            _smartContractService = smartContractService ?? throw new ArgumentNullException(nameof(smartContractService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _smartContractService.RefreshOutboundTransactions();
        }
    }
}
