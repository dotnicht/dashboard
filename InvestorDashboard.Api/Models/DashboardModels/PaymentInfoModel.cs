namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class PaymentInfoModel
    {
        public string Currency { get; set; }
        public decimal Rate { get; set; }
        public string Address { get; set; }
        public int Confirmations { get; set; }
    }
}
