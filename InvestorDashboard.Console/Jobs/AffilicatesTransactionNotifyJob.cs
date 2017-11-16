using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using InvestorDashboard.Backend.Services;

namespace InvestorDashboard.Console.Jobs
{
    public class AffilicatesTransactionNotifyJob : JobBase
    {
        private readonly IAffiliatesService _affiliatesService;

        public override TimeSpan Period => Options.Value.AffilicatesTransactionNotifyPeriod;

        public AffilicatesTransactionNotifyJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IAffiliatesService affiliatesService)
            : base(loggerFactory, context, options)
        {
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _affiliatesService.NotifyTransactionsCreated();
        }
    }
}
