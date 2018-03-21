﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly ISmartContractService _smartContractService;

        public RefreshTransactionsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices, ISmartContractService smartContractService)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _smartContractService = smartContractService ?? throw new ArgumentNullException(nameof(smartContractService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            foreach (var service in _cryptoServices)
            {
                try
                {
                    await service.RefreshInboundTransactions();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while refreshing inbound {service.Settings.Value.Currency} transactions.");
                }
            }

            await _smartContractService.RefreshOutboundTransactions();
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
