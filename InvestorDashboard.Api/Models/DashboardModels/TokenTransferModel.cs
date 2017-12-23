using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class TokenTransferModel
    {
        public string Address { get; set; }
        public string Amount { get; set; }
        public string ReCaptchaToken{ get; set; }
    }
}
