using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public class RefreshTokenBalanceJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using (var ctx = Program.ServiceProvider.GetService<ApplicationDbContext>())
            {
                var ids = ctx.Users.Select(x => x.Id).ToArray();
                foreach (var userId in ids)
                {
                    using (var service = Program.ServiceProvider.GetService<ITokenService>())
                    {
                        await service.RefreshTokenBalance(userId);
                    }
                }

                await Out.WriteLineAsync($"Token refresh completed for {ids.Length} users.");
            }
        }
    }
}
