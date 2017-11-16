using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ITokenService : IDisposable
    {
        Task RefreshTokenBalance(string userId = null);
    }
}
