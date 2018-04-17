using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IAffiliateService : IDisposable
    {
        Task NotifyTransactionsCreated();
        Task NotifyUserRegistered(ApplicationUser user);
    }
}
