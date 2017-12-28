using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class DashboardHistoryItem
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public decimal BonusPercentage { get; set; }
        public decimal TokenPrice { get; set; }
        public decimal TotalCoins { get; set; }
        public int TotalUsers { get; set; }
        public int TotalInvestors { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal TotalUsdInvested { get; set; }
        public decimal TotalCoinsBought { get; set; }
        public int TotalNonInternalUsers { get; set; }
        public int TotalNonInternalInvestors { get; set; }
        public decimal TotalNonInternalInvested { get; set; }
        public decimal TotalNonInternalUsdInvested { get; set; }
        public decimal TotalNonInternalCoinsBought { get; set; }
        public Currency Currency { get; set; }
    }
}
