using System.Collections.Generic;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class IcoInfoModel
    {
        public string TokenName { get; set; }
        public string ContractAddress { get; set; }
        public decimal TotalCoinsBought { get; set; }
        public decimal TotalUsdInvested { get; set; }
        public decimal TotalBtcInvested { get; set; }
        public decimal TotalEthInvested { get; set; }
        public decimal TotalCoins { get; set; }
        public decimal TokenPrice { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public decimal BonusPercentage { get; set; }
        public List<CurrencyValue> Currencies { get; set; }

        public class CurrencyValue
        {
            public string Currency { get; set; }
            public decimal Value { get; set; }
        }
    }
}
