using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class AffilicatesSyncJob : JobBase
    {
        private readonly IAffiliatesService _affiliatesService;

        public override TimeSpan Period => Options.Value.AffilicatesSyncPeriod;

        public AffilicatesSyncJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IAffiliatesService affiliatesService)
            : base(loggerFactory, context, options)
        {
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _affiliatesService.SyncAffiliates();
        }
    }
}
