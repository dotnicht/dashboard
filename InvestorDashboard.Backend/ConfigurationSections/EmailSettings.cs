namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class EmailSettings
    {
        public string FromAddress { get; set; }
        public string[] NotificationList { get; set; }
        public EmailProvider Provider { get; set; }
        public AmazonSettings Amazon { get; set; }
        public SendGridSettings SendGrid { get; set; }

        public enum EmailProvider
        {
            Amazon,
            SendGrid
        }

        public class AmazonSettings
        {
            public string AccessKeyId { get; set; }
            public string SecretAccessKey { get; set; }
        }

        public class SendGridSettings
        {
            public string ApiKey { get; set; }
        }
    }
}
