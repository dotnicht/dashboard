namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class TelegramSettings
    {
        public string Token { get; set; }
        public string WebhookUri { get; set; }
        public int BusinessNotificationChatId { get; set; }
        public int TechnicalNotificationChatId { get; set; }
    }
}
