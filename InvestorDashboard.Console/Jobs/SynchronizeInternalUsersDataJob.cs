using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class SynchronizeInternalUsersDataJob : JobBase
    {
        private readonly IInternalUserService _internalUserService;

        public SynchronizeInternalUsersDataJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IInternalUserService internalUserService)
            : base(loggerFactory, options)
        {
            _internalUserService = internalUserService ?? throw new ArgumentNullException(nameof(internalUserService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _internalUserService.SynchronizeInternalUsersData();
        }
    }
}
