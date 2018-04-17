using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly ISmartContractService _smartContractService;

        public RefreshTransactionsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices, ISmartContractService smartContractService)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _smartContractService = smartContractService ?? throw new ArgumentNullException(nameof(smartContractService));
        }

        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            Task.WaitAll(_cryptoServices.Select(x => x.RefreshInboundTransactions()).Union(new[] { _smartContractService.RefreshOutboundTransactions() }).ToArray());
            return Task.CompletedTask;
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
