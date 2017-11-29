using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService : IDisposable
    {
        IOptions<CryptoSettings> Settings { get; }
        Task UpdateUserDetails(string userId);
        Task RefreshInboundTransactions();
        Task TransferAssets();
    }
}
