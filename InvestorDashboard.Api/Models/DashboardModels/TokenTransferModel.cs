﻿namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class TokenTransferModel
    {
        public long Amount { get; set; }
        public string Address { get; set; }
        public string ReCaptchaToken { get; set; }
    }
}
