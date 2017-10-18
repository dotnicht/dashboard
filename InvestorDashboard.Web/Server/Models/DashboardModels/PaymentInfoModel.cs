using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Web.Server.Models.DashboardModels
{
    public class PaymentInfoModel
    {
    public string Currency { get; set; }
    public double Rate { get; set; }
    public string Address { get; set; }
    }
}
