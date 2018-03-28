using System.Collections.Generic;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class ReferralInfoModel
    {
        public string Link { get; set; }
        public Dictionary<string, ReferralCurrencyItem> Items { get; set; } = new Dictionary<string, ReferralCurrencyItem>();

        public class ReferralCurrencyItem
        {
            public string Address { get; set; }
            public decimal Pending { get; set; }
            public decimal Balance { get; set; }
            public Dictionary<string, decimal> Transactions { get; set; } = new Dictionary<string, decimal>();
        }
    }
}
