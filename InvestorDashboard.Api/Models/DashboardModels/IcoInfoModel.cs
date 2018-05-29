using InvestorDashboard.Backend.Database.Models;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class IcoInfoModel
    {
        public string Currency { get; set; }
        public string TokenName { get; set; }
        public string ContractAddress { get; set; }
        public long TotalCoinsBought { get; set; }
        public long TotalCoins { get; set; }
        public decimal TokenPrice { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsReferralSystemDisabled { get; set; }
        public decimal ReferralBonus { get; set; }
        public decimal Bonus { get; set; }
        public DateTime? BonusValidUntil { get; set; }
        public bool IsKycSystemDisabled { get; set; }
        public long? KycBonus { get; set; }
        public List<CurrencyValue> Currencies { get; set; }

        public class CurrencyValue
        {
            public Currency Currency { get; set; }
            public decimal Value { get; set; }
        }
    }
}
