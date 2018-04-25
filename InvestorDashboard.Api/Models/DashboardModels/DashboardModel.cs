namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class DashboardModel
    {
        public ClientInfoModel ClientInfoModel { get; set; }
        public PaymentInfoModel[] PaymentInfoList { get; set; }
        public IcoInfoModel IcoInfoModel { get; set; }
    }
}
