using InvestorDashboard.Backend.Models;
using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService : IDisposable
    {
        IOptions<CryptoSettings> Settings { get; }
        Task UpdateUserDetails(string userId);
        Task RefreshInboundTransactions();
        Task TransferAssets(string destinationAddress);
    }
}
