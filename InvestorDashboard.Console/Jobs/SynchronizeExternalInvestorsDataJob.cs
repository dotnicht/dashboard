using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class SynchronizeExternalInvestorsDataJob : JobBase
    {
        private readonly IExternalInvestorService _investorsService;

        public SynchronizeExternalInvestorsDataJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IExternalInvestorService investorsService)
            : base(loggerFactory, options)
        {
            _investorsService = investorsService ?? throw new ArgumentNullException(nameof(investorsService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _investorsService.SynchronizeInvestorsData();
        }
    }
}
