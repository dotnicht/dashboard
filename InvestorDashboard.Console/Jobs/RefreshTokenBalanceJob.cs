using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTokenBalanceJob : JobBase
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenBalanceJob(ILoggerFactory loggerFactory, IOptions<JobsSettings> options, ITokenService tokenService)
            : base(loggerFactory, options)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _tokenService.RefreshTokenBalance();
        }
    }
}
