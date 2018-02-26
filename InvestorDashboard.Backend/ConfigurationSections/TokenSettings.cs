namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class TokenSettings
    {
        public string TokenName { get; set; }
        public decimal TotalCoins { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsTokenTransferDisabled { get; set; }
        public decimal Price { get; set; }
        public decimal BonusPercentage { get; set; }
        public int OutboundTransactionsLimit { get; set; }
    }
}
