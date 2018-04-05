namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class ReferralInfoModel
    {
        public string Link { get; set; }
        public ReferralCurrencyItem[] Items { get; set; }

        public class ReferralCurrencyItem
        {
            public string Currency { get; set; }
            public string Address { get; set; }
            public decimal Pending { get; set; }
            public decimal Balance { get; set; }
            public ReferralCurrencyTransaction[] Transactions { get; set; }

            public class ReferralCurrencyTransaction
            {
                public string Hash { get; set; }
                public decimal Amount { get; set; }
            }
        }
    }
}
