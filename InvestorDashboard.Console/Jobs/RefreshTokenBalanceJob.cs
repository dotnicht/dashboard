using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Options;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTokenBalanceJob : JobBase
    {
        private readonly ITokenService _tokenService;

        public override TimeSpan Period => Options.Value.RefreshTokenBalancePeriod;

        public RefreshTokenBalanceJob(ApplicationDbContext context, IOptions<JobsSettings> options, ITokenService tokenService) 
            : base(context, options)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var ids = Context.Users.Select(x => x.Id).ToArray();
            foreach (var userId in ids)
            {
                await _tokenService.RefreshTokenBalance(userId);
            }

            await Out.WriteLineAsync($"Token refresh completed for {ids.Length} users.");
        }
    }
}
