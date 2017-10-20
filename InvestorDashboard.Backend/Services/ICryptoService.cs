using InvestorDashboard.Backend.Models;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService
    {
        Currency Currency { get; }
        Task UpdateUserDetails(string userId);
        Task RefreshInboundTransactions();
    }
}
