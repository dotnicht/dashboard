﻿namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class ClientInfoModel
    {
        public string Address { get; set; }
        public long Balance { get; set; }
        public long BonusBalance { get; set; }
        public bool ThresholdExceeded { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsEligibleForTransfer { get; set; }
        public bool IsInvestor { get; set; }
        public long TokensAvailableForTransfer { get; set; }
        public bool IsAdmin { get; set; }
    }
}
