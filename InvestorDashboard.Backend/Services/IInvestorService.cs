using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInvestorService : IDisposable
    {
        Task<int> LoadInvestorsData();
        Task<int> ActivateInvestors();
        Task<int> ClearInvestors();
    }
}
