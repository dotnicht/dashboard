using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IAffiliatesService
    {
        Task SyncAffiliates();
        Task<decimal> GetUserAffilicateBalance(string userId);
    }
}
