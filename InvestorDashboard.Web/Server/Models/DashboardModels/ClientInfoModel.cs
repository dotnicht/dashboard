namespace InvestorDashboard.Web.Server.Models.DashboardModels
{
    public class ClientInfoModel
    {
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public decimal BonusBalance { get; set; }
        public bool IsTokenSaleDisabled{ get; set; }
    }
}
