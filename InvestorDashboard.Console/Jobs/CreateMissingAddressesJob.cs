using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class CreateMissingAddressesJob : JobBase
    {
        private readonly IGenericAddressService _genericAddressService;

        public CreateMissingAddressesJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IGenericAddressService genericAddressService)
            : base(loggerFactory, options)
        {
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _genericAddressService.CreateMissingAddresses();
        }
    }
}
