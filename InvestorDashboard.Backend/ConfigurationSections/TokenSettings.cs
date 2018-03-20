using InvestorDashboard.Backend.Database.Models;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class TokenSettings
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public Currency Currency { get; set; }
        public long TotalCoins { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsTokenTransferDisabled { get; set; }
        public int OutboundTransactionsLimit { get; set; }
        public long BalanceThreshold { get; set; }
        public BonusSettings Bonus { get; set; }

        public class BonusSettings
        {
            public BonusSystem System { get; set; }
            public ScheduledItem[] Scheduled { get; set; }
            public PercentageItem[] Percentage { get; set; }

            public enum BonusSystem
            {
                None,
                Scheduled,
                Percentage
            }

            public class ScheduledItem
            {
                public DateTime? Start { get; set; }
                public DateTime? End { get; set; }
                public decimal Amount { get; set; }
            }

            public class PercentageItem
            {
                public long? Lower { get; set; }
                public long? Upper { get; set; }
                public decimal Amount { get; set; }
            }
        }
    }
}
