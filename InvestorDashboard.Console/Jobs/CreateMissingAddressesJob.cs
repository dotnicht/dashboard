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
    public class CreateMissingAddressesJob : JobBase
    {
        private readonly IGenericAddressService _genericAddressService;

        public CreateMissingAddressesJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IGenericAddressService genericAddressService)
            : base(loggerFactory, context, options)
        {
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _genericAddressService.CreateMissingAddresses();
        }

        protected override void Dispose(bool disposing)
        {
            _genericAddressService.Dispose();
            base.Dispose(disposing);
        }
    }
}
