using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IKycService
    {
        Task UpdateKycTransactions(string userId = null);
        Task DetectDuplicateKycData(string userId = null);
        Task<CryptoTransaction[]> GetKycTransactions(string userId, Guid hash);
    }
}
