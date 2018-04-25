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

        public RefreshDashboardHistoryJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IDashboardHistoryService dashboardHistoryService)
            : base(loggerFactory, options)
        {
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _dashboardHistoryService.RefreshHistory();
        }
    }
}
