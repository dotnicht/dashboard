using InvestorDashboard.Backend.Database.Models;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ISmartContractService
    {
        Task<(string Hash, bool Success)> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, BigInteger amount);
        Task<(string Hash, bool Success)> CallSmartContractMintTokensFunction(string destinationAddress, BigInteger amount);
        Task<BigInteger> CallSmartContractBalanceOfFunction(string address);
        Task<bool?> GetTransactionReceipt(string hash);
    }
}
