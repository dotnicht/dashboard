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

namespace InvestorDashboard.Console.Jobs
{
    public class TransferCryptoAssetsJob : JobBase
    {
        public override TimeSpan Period => Options.Value.TransferCryptoAssetsPeriod;

        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public TransferCryptoAssetsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
