using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService
    {
        Task SynchronizeInternalUsersData();
        Task<(Guid Id, CryptoTransaction[] Transactions)?> GetManagementTransactions(string email);
        Task<(Guid Id, CryptoTransaction[] Transactions)?> GetManagementTransactions(Guid userId);
        Task AddManagementTransaction(Guid userId, long amount);
    }
}
