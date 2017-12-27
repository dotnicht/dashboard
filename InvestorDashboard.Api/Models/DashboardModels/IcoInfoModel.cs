namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class IcoInfoModel
    {
        public int TotalInvestors { get; set; }
        public decimal TotalCoinsBought { get; set; }
        public decimal TotalUsdInvested { get; set; }
        public decimal TotalBtcInvested { get; set; }
        public decimal TotalEthInvested { get; set; }
        public decimal TotalCoins { get; set; }
        public decimal TokenPrice { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public decimal BonusPercentage { get; set; }
    }
}
