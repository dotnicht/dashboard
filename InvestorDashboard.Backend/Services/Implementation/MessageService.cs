using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class MessageService : ContextService, IMessageService
    {
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly IEmailService _emailService;
        private readonly ITelegramService _telegramService;

        public MessageService(ApplicationDbContext context, ILoggerFactory loggerFactory, IDashboardHistoryService dashboardHistoryService, IEmailService emailService, ITelegramService telegramService) 
            : base(context, loggerFactory)
        {
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
        }

        public async Task HandleIncomingMessage(string user, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Logger.LogInformation($"Handle incoming message from user { user }. Text: { message }");

            var msg = message.Trim();
            var commands = new Dictionary<string, Func<Task>>
            {
                { "/ping", () => _telegramService.SendMessage("pong") },
                { "/pong", () => _telegramService.SendMessage("ping") },
                { "/status", () => SendDashboardHistoryMessage() }
            };

            foreach (var item in commands)
            {
                if (string.Compare(msg, item.Key, true) == 0)
                {
                    await item.Value();
                    break;
                }
            }
        }

        public async Task SendRegistrationConfirmationRequiredMessage(string userId, string message)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var user = Context.Users.Single(x => x.Id == userId);
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", message);
        }

        public async Task SendDashboardHistoryMessage()
        {
            var item = await _dashboardHistoryService.GetLatestHistoryItem();
            await _telegramService.SendMessage($"Status on { item.Created } | Total investors: { item.TotalNonInternalInvestors } | Total USD: { item.TotalNonInternalUsdInvested } | Total users: { item.TotalNonInternalUsers }");
        }
    }
}
