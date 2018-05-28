using InvestorDashboard.Backend.Database.Models;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class PaymentInfoModel
    {
        public Currency Currency { get; set; }
        public decimal Rate { get; set; }
        public string Address { get; set; }
        public int Confirmations { get; set; }
        public bool IsDisabled { get; set; }
    }
}
