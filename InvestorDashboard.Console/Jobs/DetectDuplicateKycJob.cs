using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class DetectDuplicateKycJob : JobBase
    {
        private readonly IKycService _kycService;

        public DetectDuplicateKycJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IKycService kycService)
            : base(loggerFactory, options)
        {
            _kycService = kycService ?? throw new ArgumentNullException(nameof(kycService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _kycService.DetectDuplicateKycData();
        }
    }
}
