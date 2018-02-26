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
        private readonly IMessageService _messageService;

        public NotifyDashboardHistoryJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IMessageService messageService) 
            : base(loggerFactory, context, options)
        {
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            await _messageService.SendDashboardHistoryMessage();
        }

        protected override void Dispose(bool disposing)
        {
            _messageService.Dispose();
            base.Dispose(disposing);
        }
    }
}
