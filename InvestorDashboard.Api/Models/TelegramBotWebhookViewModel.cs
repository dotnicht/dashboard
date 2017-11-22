namespace InvestorDashboard.Api.Models
{
    public class TelegramBotWebhookViewModel
    { 
        public int Update_id { get; set; }
        public InnerMessage Message { get; set; }

        public class InnerMessage
        {
            public int Date { get; set; }
            public Chat Chat { get; set; }
            public int Message_id { get; set; }
            public From From { get; set; }
            public string Text { get; set; }
        }

        public class Chat
        {
            public string Last_name { get; set; }
            public int Id { get; set; }
            public string Type { get; set; }
            public string First_name { get; set; }
            public string Username { get; set; }
        }

        public class From
        {
            public string Last_name { get; set; }
            public int Id { get; set; }
            public string First_name { get; set; }
            public string Username { get; set; }
        }
    }
}
