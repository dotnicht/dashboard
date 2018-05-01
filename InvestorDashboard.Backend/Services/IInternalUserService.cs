using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService
    {
        Task SynchronizeInternalUsersData();
        Task UpdateKycTransactions(string userId = null);
        Task DetectDuplicateKycData(string userId = null);
    }
}
