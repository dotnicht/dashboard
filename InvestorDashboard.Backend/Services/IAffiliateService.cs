using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IAffiliateService
    {
        Task NotifyTransactionsCreated();
        Task NotifyUserRegistered(string userId = null);
    }
}
