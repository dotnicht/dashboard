using InvestorDashboard.Backend.Database.Models;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService : ICryptoService
    {
        Task<(string Hash, bool Success)> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, decimal amount);
    }
}
