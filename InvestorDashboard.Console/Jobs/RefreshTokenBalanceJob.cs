using System;
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
    public class RefreshTokenBalanceJob : JobBase
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenBalanceJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, ITokenService tokenService) 
            : base(loggerFactory, context, options)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _tokenService.RefreshTokenBalance();
        }

        protected override void Dispose(bool disposing)
        {
            _tokenService.Dispose();
            base.Dispose(disposing);
        }
    }
}
