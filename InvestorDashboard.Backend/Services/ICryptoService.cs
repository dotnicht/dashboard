using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Options;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService : IDisposable
    {
        IOptions<CryptoSettings> Settings { get; }
        Task<CryptoAddress> CreateCryptoAddress(string userId, string password = null);
        Task RefreshInboundTransactions();
        Task RefreshTransactionsFromBlockchain();
        Task TransferAvailableAssets();
        Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransaction(CryptoAddress sourceAddress, string destinationAddress, BigInteger? amount = null);
    }
}
