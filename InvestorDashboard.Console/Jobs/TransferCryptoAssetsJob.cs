using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class TransferCryptoAssetsJob : JobBase
    {
        public override TimeSpan Period => TimeSpan.FromMinutes(1);

        private readonly ApplicationDbContext _context;

        public TransferCryptoAssetsJob(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
        }
    }
}
