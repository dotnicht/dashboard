using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class NotifyAffilicatesJob : JobBase
    {
        private readonly IAffiliateService _affiliatesService;

        public NotifyAffilicatesJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IAffiliateService affiliatesService)
            : base(loggerFactory, options)
        {
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            Task.WaitAll(_affiliatesService.NotifyTransactionsCreated(), _affiliatesService.NotifyUserRegistered());
            return Task.CompletedTask;
        }
    }
}
