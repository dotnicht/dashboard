using InvestorDashboard.Backend.ConfigurationSections;
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
        private readonly IOptions<TelegramSettings> _telegramSettings;
        private readonly IOptions<EmailSettings> _emailSettings;

        public MessageService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IDashboardHistoryService dashboardHistoryService,
            IEmailService emailService,
            ITelegramService telegramService,
            IOptions<TelegramSettings> telegramSettings,
            IOptions<EmailSettings> emailSettings)
            : base(serviceProvider, loggerFactory)
        {
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _telegramService = telegramService ?? throw new ArgumentNullException(nameof(telegramService));
            _telegramSettings = telegramSettings ?? throw new ArgumentNullException(nameof(telegramSettings));
            _emailSettings = emailSettings;
        }

        public async Task HandleIncomingMessage(string externalUserName, string message, int chatId)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (chatId != _telegramSettings.Value.BusinessNotificationChatId && chatId != _telegramSettings.Value.TechnicalNotificationChatId)
            {
                throw new InvalidOperationException($"Unsupported chat id {chatId}. User: {externalUserName}. Message: {message}.");
            }

            Logger.LogInformation($"Handle incoming message from user {externalUserName}. Text: {message}.");

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

            using (var ctx = CreateContext())
            {
                var user = ctx.Users.Single(x => x.Id == userId);
                await _emailService.SendEmailAsync(new[] { user.Email }, "Confirm Your Email", message);
            }
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

            using (var ctx = CreateContext())
            {
                var user = ctx.Users.Single(x => x.Id == userId);
                await _emailService.SendEmailAsync(new[] { user.Email }, "Reset Password", message);
            }
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
                sb.AppendLine($"Users: {items.First().Value.TotalNonInternalUsers}");
                sb.AppendLine($"Investors: {items.First().Value.TotalNonInternalInvestors}");
                sb.AppendLine($"Coins: {items.First().Value.TotalNonInternalCoinsBoughts}");
                sb.AppendLine(string.Join(Environment.NewLine, items.Select(x => $"{x.Key}: {x.Value.TotalNonInternalInvested}")));
            }
            else
            {
                sb.AppendLine("Dashboard history is empty.");
            }

            var msg = sb.ToString();

            try
            {
                await _telegramService.SendMessage(msg, chatId ?? _telegramSettings.Value.BusinessNotificationChatId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while sending dashboard history message to telegram.");
                await _emailService.SendEmailAsync(_emailSettings.Value.NotificationList, "Dashboard History Notification", msg.Replace(Environment.NewLine, "<br />"));
            }

            sb.Clear();

            // TODO: add detailed technical info for dev chat.
        }
    }
}
