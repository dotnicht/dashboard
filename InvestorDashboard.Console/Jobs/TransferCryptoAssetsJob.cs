using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Options;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class TransferCryptoAssetsJob : JobBase
    {
        public override TimeSpan Period => Options.Value.TransferCryptoAssetsPeriod;

        public TransferCryptoAssetsJob(ApplicationDbContext context, IOptions<JobsSettings> options)
            : base(context, options)
        {
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
        }
    }
}
