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
        private readonly IDashboardHistoryService _dashboardHistoryService;

        public TelegramService(ILogger<TelegramService> logger, IOptions<TelegramSettings> options, IDashboardHistoryService dashboardHistoryService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
        }

        public async Task SendDashboardHistoryMessage()
        {
            var item = await _dashboardHistoryService.GetLatestHistoryItem();
            await SendMessage($"Status on { item.Created } | Total investors: { item.TotalNonInternalInvestors } | Total USD: { item.TotalNonInternalUsdInvested }");
        }

        public async Task SendMessage(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var client = new TelegramBotClient(_options.Value.Token);
            var chatId = new ChatId(_options.Value.ChatId);

            await client.SetWebhookAsync(_options.Value.WebhookUri);
            await client.SendTextMessageAsync(chatId, message);
        }

        public async Task HandleIncomingMessage(string user, string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.Compare(user, "@McKlavishnikov", true) == 0)
            {
                await SendMessage("Женя, отстань.");
            }
            else if (string.Compare(message.Trim(), "ping", true) == 0)
            {
                await SendMessage("pong");
            }
        }
    }
}
