using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService
    {
        Task SynchronizeInternalUsersData();
    }
}
