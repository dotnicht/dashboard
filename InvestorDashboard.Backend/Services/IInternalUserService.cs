using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService
    {
        Task SynchronizeInternalUsersData();
        Task<(Guid Id, CryptoTransaction[] Transactions)> GetManagementTransactions(string email);
        Task<string> AddManagementTransaction(Guid userId, long amount);
    }
}
