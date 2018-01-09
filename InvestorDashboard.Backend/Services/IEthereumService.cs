using InvestorDashboard.Backend.Database.Models;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService : ICryptoService
    {
        Task<bool> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, decimal amount);
    }
}
