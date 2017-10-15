using InvestorDashboard.Backend.Models;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService
    {
        EthereumAccount CreateAccount();
        IEnumerable<EthereumTransaction> GetInboundTransactionsByRecipientAddress(string address);
    }
}
