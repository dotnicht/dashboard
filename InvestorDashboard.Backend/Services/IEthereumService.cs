using InvestorDashboard.Backend.Models;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService
    {
        EthereumAccount CreateAccount();
        EthereumTransaction[] GetInboundTransactionsByRecipientAddress(string address);
    }
}
