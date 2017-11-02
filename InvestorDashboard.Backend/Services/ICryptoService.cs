using InvestorDashboard.Backend.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService : IDisposable
    {
        Currency Currency { get; }
        Task UpdateUserDetails(string userId);
        Task RefreshInboundTransactions();
        Task TransferAssets(string destinationAddress);
    }
}
