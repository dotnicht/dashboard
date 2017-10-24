using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTokenBalanceJob : JobBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public override TimeSpan Period => TimeSpan.FromMinutes(1);

        public RefreshTokenBalanceJob(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            var ids = _context.Users.Select(x => x.Id).ToArray();
            foreach (var userId in ids)
            {
                await _tokenService.RefreshTokenBalance(userId);
            }

            await Out.WriteLineAsync($"Token refresh completed for {ids.Length} users.");
        }
    }
}
