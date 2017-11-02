﻿using System;
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

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            // TODO: determine address for crypto assets transfer.
            // _cryptoServices.ToList().ForEach(x => x.TransferAssets(null).Wait());
            Logger.LogInformation($"Crypto assets transfer completed for currencies: { string.Join(", ", _cryptoServices.Select(x => x.Currency.ToString())) }");
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
