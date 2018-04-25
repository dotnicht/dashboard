using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService
    {
        Task SynchronizeInternalUsersData();
        Task UpdateKycTransaction(string userId = null);
        bool IsKycDataFilled(ApplicationUser user);
    }
}
