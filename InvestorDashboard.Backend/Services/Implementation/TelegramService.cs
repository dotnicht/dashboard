using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TelegramService : ITelegramService
    {
        private readonly ILogger<TelegramService> _logger;
        private readonly IOptions<TelegramSettings> _options;

        public TelegramService(ILogger<TelegramService> logger, IOptions<TelegramSettings> options, IDashboardHistoryService dashboardHistoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SendMessage(string message, int chatId)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var client = new TelegramBotClient(_options.Value.Token);
            var chat = new ChatId(chatId);

            await client.SetWebhookAsync(_options.Value.WebhookUri);
            await client.SendTextMessageAsync(chat, message);
        }
    }
}
