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
    public class RefreshDashboardHistoryJob : JobBase
    {
        private readonly IDashboardHistoryService _dashboardHistoryService;

        public override TimeSpan Period => Options.Value.RefreshDashboardHistoryPeriod;

        public RefreshDashboardHistoryJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IDashboardHistoryService dashboardHistoryService)
            : base(loggerFactory, context, options)
        {
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _dashboardHistoryService.RefreshHistory();
        }

        protected override void Dispose(bool disposing)
        {
            _dashboardHistoryService.Dispose();
            base.Dispose(disposing);
        }
    }
}
