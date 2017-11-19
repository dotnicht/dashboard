using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.EntityFrameworkCore;
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
            var users = Context.Users
                .Include(x => x.CryptoAddresses)
                .Where(x => x.CryptoAddresses.Count > 3)
                .ToArray();

            foreach (var user in users)
            {
                Disable(user.CryptoAddresses.Where(x => x.Currency == Currency.BTC).ToArray());
                Disable(user.CryptoAddresses.Where(x => x.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment).ToArray());
                Disable(user.CryptoAddresses.Where(x => x.Currency == Currency.ETH && x.Type == CryptoAddressType.Contract).ToArray());
            }
        }

        private void Disable(CryptoAddress[] addresses)
        {
            addresses.Select((a, i) => a.IsDisabled = i != 0).ToArray();
            Context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
