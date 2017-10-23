using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ITokenService : IDisposable
    {
        Task<decimal> RefreshTokenBalance(string userId);
    }
}
