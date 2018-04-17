using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IAffiliateService : IDisposable
    {
        Task NotifyTransactionsCreated();
        Task NotifyUsersRegistered();
    }
}
