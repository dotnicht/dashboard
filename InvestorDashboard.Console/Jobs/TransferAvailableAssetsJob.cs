using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Microsoft.EntityFrameworkCore;

namespace InvestorDashboard.Console.Jobs
{
    public class TransferAvailableAssetsJob : JobBase
    {
        public override TimeSpan Period => Options.Value.TransferAvailableAssetsPeriod;

        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public TransferAvailableAssetsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {

            var eth = _cryptoServices.Single(x => x.Settings.Value.Currency == Backend.Database.Models.Currency.ETH);
            var user = Context.Users.Include(x => x.CryptoAddresses).Single(x => x.Email == "dotnicht@live.com");
            var address = user.CryptoAddresses.Single(x => x.Currency == Backend.Database.Models.Currency.ETH);
            var hash = await eth.PublishTransaction(address, "0xe8141d6cfA2052E853C47E030f64dB649d47DF1f");

            return;

            foreach (var service in _cryptoServices)
            {
                try
                {
                    await service.TransferAvailableAssets();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while transfering { service.Settings.Value.Currency } assets.");
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
