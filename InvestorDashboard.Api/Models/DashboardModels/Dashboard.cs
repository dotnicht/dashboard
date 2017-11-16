using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class Dashboard
    {
        public ClientInfoModel clientInfoModel { get; set; }
        public List<PaymentInfoModel> paymentInfoList { get; set; }
        public IcoInfoModel icoInfoModel { get; set; }
    }
}
