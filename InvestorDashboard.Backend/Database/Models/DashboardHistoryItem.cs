﻿using InvestorDashboard.Backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class DashboardHistoryItem
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public int TotalUsers { get; set; }
        public int TotalInvestors { get; set; }
        public decimal TotalUsdInvested { get; set; }
        public decimal TotalCoinsBought { get; set; }
        public decimal TotalCoins { get; set; }
        public decimal TokenPrice { get; set; }
        public decimal BonusPercentage { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public int TotalNonInternalUsers { get; set; }
        public int TotalNonInternalInvestors { get; set; }
        public decimal TotalNonInternalUsdInvested { get; set; }
        [NotMapped]
        public IEnumerable<(Currency Currency, decimal Amount)> Currencies { get; set; }
    }
}
