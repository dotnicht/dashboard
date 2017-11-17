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
    public class InternalUsersSyncJob : JobBase
    {
        private readonly IInternalUserService _internalUserService;

        public override TimeSpan Period => Options.Value.InternalUsersSyncPeriod;

        public InternalUsersSyncJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IInternalUserService internalUserService)
            : base(loggerFactory, context, options)
        {
            _internalUserService = internalUserService ?? throw new ArgumentNullException(nameof(internalUserService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _internalUserService.SyncInternalUsers();
        }
    }
}
