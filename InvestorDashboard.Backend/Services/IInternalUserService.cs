using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService : IDisposable
    {
        Task SyncInternalUsers();
        Task<decimal> GetInternalUserBalance(string userId);
    }
}
