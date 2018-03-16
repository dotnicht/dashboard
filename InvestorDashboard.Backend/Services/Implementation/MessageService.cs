using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (chatId != _options.Value.BusinessNotificationChatId && chatId != _options.Value.TechnicalNotificationChatId)
            {
                throw new InvalidOperationException($"Unsupported chat id {chatId}. User: {user}. Message: {message}.");
            }

            Logger.LogInformation($"Handle incoming message from user {user}. Text: {message}.");

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

        public async Task SendPasswordResetMessage(string userId, string message)
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
            await _emailService.SendEmailAsync(user.Email, "Reset Password", message);
        }

        public Task SendDashboardHistoryMessage()
        {
            return SendDashboardHistoryMessageInternal();
        }

        private async Task SendDashboardHistoryMessageInternal(int? chatId = null)
        {
            var items = await _dashboardHistoryService.GetHistoryItems();

            var sb = new StringBuilder();

            sb.AppendLine($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

            if (items.Any())
            {
                sb.AppendLine($"Total users: {items.First().Value.TotalNonInternalUsers}");
                sb.AppendLine($"Total investors: {items.Sum(x => x.Value.TotalNonInternalInvestors)}");
                sb.AppendLine(string.Join(Environment.NewLine, items.Select(x => $"Total {x.Key}: {x.Value.TotalNonInternalInvested}")));
            }
            else
            {
                sb.AppendLine("Dashboard history is empty.");
            }

            await _telegramService.SendMessage(sb.ToString(), chatId ?? _options.Value.BusinessNotificationChatId);
        }
    }
}
