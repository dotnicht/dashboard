namespace InvestorDashboard.Backend.Models
{
    public class EthereumTransaction
    {
        public string Hash { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public decimal Amount { get; set; }

    }
}
