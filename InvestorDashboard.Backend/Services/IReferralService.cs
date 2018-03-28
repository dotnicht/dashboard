using InvestorDashboard.Backend.Database.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IReferralService
    {
        Task<(Dictionary<string, decimal> Transactions, decimal Pending, decimal Balance)> GetRererralData(string userId, Currency currency);
        Task PopulateReferralData(ApplicationUser user, string referralCode);
        Task UpdateReferralAddress(ApplicationUser user, Currency currency, string address);
    }
}
