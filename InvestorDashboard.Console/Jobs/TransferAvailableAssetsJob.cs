using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
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
