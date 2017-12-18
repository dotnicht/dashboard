using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInternalUserService : IDisposable
    {
        Task SynchronizeInternalUsersData();
    }
}
