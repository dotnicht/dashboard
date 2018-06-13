using InvestorDashboard.Backend.Database.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using static InvestorDashboard.Backend.ConfigurationSections.TokenSettings.BonusSettings;

namespace InvestorDashboard.Backend.Services
{
    public interface IKycService
    {
        Task UpdateKycTransactions(string userId = null);
        Task DetectDuplicateKycData(string userId = null);
        Task<CryptoTransaction[]> GetKycTransactions(string userId, params Guid[] hash);
        Task<Dictionary<BonusCriterion, (bool Status, long Amount)>> UpdateUserKycData(ApplicationUser user);
        Task<Dictionary<BonusCriterion, (bool Status, long Amount)>> GetUserKycDataStatus(string userId);
        Task<BigInteger> GetAllowedNonInvestorTransferAmount(string userId);
    }
}
