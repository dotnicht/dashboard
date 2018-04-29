using InvestorDashboard.Backend.ConfigurationSections;
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
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public TransferAvailableAssetsJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            Task.WaitAll(_cryptoServices.Select(x => x.TransferAvailableAssets()).ToArray());
            return Task.CompletedTask;
        }
    }
}
