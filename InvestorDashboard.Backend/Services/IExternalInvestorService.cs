using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IExternalInvestorService
    {
        Task SynchronizeInvestorsData();
    }
}
