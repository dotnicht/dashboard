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
    public class CreateMissingInternalTransactionsJob : JobBase
    {
        private readonly IInternalUserService _internalUserService;

        public CreateMissingInternalTransactionsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IInternalUserService internalUserService)
            : base(loggerFactory, context, options)
        {
            _internalUserService = internalUserService ?? throw new ArgumentNullException(nameof(internalUserService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _internalUserService.UpdateKycTransaction();
        }

        protected override void Dispose(bool disposing)
        {
            _internalUserService.Dispose();
            base.Dispose(disposing);
        }
    }
}
