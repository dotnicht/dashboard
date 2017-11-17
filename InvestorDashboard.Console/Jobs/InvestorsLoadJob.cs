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
    public class InvestorsLoadJob : JobBase
    {
        private readonly IInvestorService _investorsService;

        public override TimeSpan Period => Options.Value.InvestorsLoadPeriod;

        public InvestorsLoadJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IInvestorService investorsService)
            : base(loggerFactory, context, options)
        {
            _investorsService = investorsService ?? throw new ArgumentNullException(nameof(investorsService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            Logger.LogInformation($"Total { await _investorsService.LoadInvestorsData() } users loaded.");
        }

        protected override void Dispose(bool disposing)
        {
            _investorsService.Dispose();
            base.Dispose(disposing);
        }
    }
}
