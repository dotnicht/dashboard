namespace InvestorDashboard.Web.Server.Models.DashboardModels
{
    public class IcoInfoModel
    {
        public int TotalInvestors { get; set; }
        public decimal TotalUsdInvested { get; set; }
        public decimal TotalCoinsBought { get; set; }
        public decimal TotalCoins { get; set; }
        public bool SellDisabled { get; set; }
    }
}
