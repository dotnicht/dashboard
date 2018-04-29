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
    public class RefreshExchangeRatesJob : JobBase
    {
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public RefreshExchangeRatesJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, IOptions<TokenSettings> tokenSettings, IExchangeRateService exchangeRateService, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, options)
        {
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            Task.WaitAll(_cryptoServices.Select(x => _exchangeRateService.RefreshExchangeRate(x.Settings.Value.Currency, _tokenSettings.Value.Currency)).ToArray());
            return Task.CompletedTask;
        }
    }
}
