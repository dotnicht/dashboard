using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly IOptions<TelegramSettings> _options;

        public MessageService(
            ApplicationDbContext context, 
            ILoggerFactory loggerFactory, 
            IDashboardHistoryService dashboardHistoryService, 
            IEmailService emailService, 
            ITelegramService telegramService,
            IOptions<TelegramSettings> options) 
            : base(context, loggerFactory)
        {
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task HandleIncomingMessage(string user, string message, int chatId)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Logger.LogInformation($"Handle incoming message from user { user }. Text: { message }");

            var msg = message.Trim();
            var commands = new Dictionary<string, Func<Task>>
            {
                { "/ping", () => _telegramService.SendMessage("pong", chatId) },
                { "/pong", () => _telegramService.SendMessage("ping", chatId) },
                { "/status", () => SendDashboardHistoryMessageInternal(chatId) }
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

        public async Task SendPasswordResetMessage(string email, string message)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var user = Context.Users.Single(x => x.Id == email);
            await _emailService.SendEmailAsync(user.Email, "Reset Password", message);
        }

        public Task SendDashboardHistoryMessage()
        {
            return SendDashboardHistoryMessageInternal();
        }

        private async Task SendDashboardHistoryMessageInternal(int? chatId = null)
        {
            var item = await _dashboardHistoryService.GetLatestHistoryItem(true);
            var msg = item + $" | Environment: { Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") }";
            await _telegramService.SendMessage(msg, chatId ?? _options.Value.BusinessNotificationChatId);
        }
    }
}
