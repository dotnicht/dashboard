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
    public class SynchronizeInternalUsersDataJob : JobBase
    {
        private readonly IInternalUserService _internalUserService;

        public SynchronizeInternalUsersDataJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IInternalUserService internalUserService)
            : base(loggerFactory, context, options)
        {
            _internalUserService = internalUserService ?? throw new ArgumentNullException(nameof(internalUserService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _internalUserService.SynchronizeInternalUsersData();
        }

        protected override void Dispose(bool disposing)
        {
            _internalUserService.Dispose();
            base.Dispose(disposing);
        }
    }
}
