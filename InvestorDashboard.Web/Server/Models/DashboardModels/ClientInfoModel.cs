using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Web.Server.Models.DashboardModels
{
    public class ClientInfoModel
    {
        public string Address { get; set; }
        public decimal Balance { get; set; }
        public bool IsTokenSaleDisabled{ get; set; }
    }
}
