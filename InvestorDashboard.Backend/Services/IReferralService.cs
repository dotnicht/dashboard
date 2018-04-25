using InvestorDashboard.Backend.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IReferralService
    {
        Task<(Dictionary<string, decimal> Transactions, decimal Pending, decimal Balance)> GetRererralData(string userId, Currency currency);
        Task PopulateReferralData(string userId, string referralCode);
        Task UpdateReferralAddress(string userId, Currency currency, string address);
    }
}
