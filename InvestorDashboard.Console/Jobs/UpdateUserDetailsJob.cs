using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class UpdateUserDetailsJob : JobBase
    {
        public override TimeSpan Period => Options.Value.UpdateUserDetailsPeriod;

        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public UpdateUserDetailsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            foreach (var service in _cryptoServices)
            {
                Context.Users
                    .Where(x => !x.CryptoAddresses.Any(y => !y.IsDisabled && y.Type == CryptoAddressType.Investment && y.Currency == service.Settings.Value.Currency))
                    .Select(x => x.Id)
                    .ToList()
                    .ForEach(async x => await service.UpdateUserDetails(x));
            }
        }
    }
}
