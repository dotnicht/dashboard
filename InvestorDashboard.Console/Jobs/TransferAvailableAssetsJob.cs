using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var address = Context.Users
                .Include(x => x.CryptoAddresses)
                .Single(x => x.Email == "125403607@qq.com")
                .CryptoAddresses
                .Single(x => x.Currency == Backend.Database.Models.Currency.BTC);

            var service = _cryptoServices.Single(x => x.Settings.Value.Currency == Backend.Database.Models.Currency.BTC);
            var (Hash, AdjustedAmount, Success) = await service.PublishTransaction(address, "3Kc86CM37FYNg3y2rL5M5YLi66UavwKSaK");

            Logger.LogDebug($" Hash: { Hash } ");

            /*
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
            */
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
