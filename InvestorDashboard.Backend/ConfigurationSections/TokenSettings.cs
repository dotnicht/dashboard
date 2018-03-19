using InvestorDashboard.Backend.Database.Models;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class TokenSettings
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Currency Currency { get; set; }
        public decimal TotalCoins { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsTokenTransferDisabled { get; set; }
        public decimal BonusPercentage { get; set; }
        public int OutboundTransactionsLimit { get; set; }
    }
}
