using System.Collections.Generic;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class Dashboard
    {
        public ClientInfoModel ClientInfoModel { get; set; }
        public List<PaymentInfoModel> PaymentInfoList { get; set; }
        public IcoInfoModel IcoInfoModel { get; set; }
    }
}
