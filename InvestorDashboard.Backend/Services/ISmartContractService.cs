using InvestorDashboard.Backend.Database.Models;
using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ISmartContractService : IDisposable
    {
        Task<(string Hash, bool Success)> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, decimal amount);
        Task<decimal> CallSmartContractBalanceOfFunction(string address);
        Task RefreshOutboundTransactions();
    }
}
