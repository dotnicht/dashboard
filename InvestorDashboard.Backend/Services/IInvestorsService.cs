using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IInvestorsService : IDisposable
    {
        Task<int> LoadInvestorsData();
        Task<int> ActivateInvestors();
        Task<int> ClearInvestors();
    }
}
