using InvestorDashboard.Backend.Models;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService : ICryptoService
    {
        EthereumAccount CreateAccount();
    }
}
