using InvestorDashboard.Backend.Database.Models;
using System.Collections.Generic;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class ReferralInfoModel
    {
        public string Link { get; set; }
        public Dictionary<Currency, ReferralCurrencyItem> Items { get; set; }

        public class ReferralCurrencyItem
        {
            public string Address { get; set; }
            public decimal Balance { get; set; }
            public decimal Pending { get; set; }
            public Dictionary<string, decimal> Transactions { get; set; }
        }
    }
}
