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
    public class NotifyDashboardHistoryJob : JobBase
    {
        private readonly ITelegramService _telegramService;

        public override TimeSpan Period => Options.Value.NotifyDashboardHistoryPeriod;

        public NotifyDashboardHistoryJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, ITelegramService telegramService) 
            : base(loggerFactory, context, options)
        {
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _telegramService.SendDashboardHistoryMessage();
        }
    }
}
