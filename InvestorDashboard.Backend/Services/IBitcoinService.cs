using InvestorDashboard.Backend.Services.Implementation;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IBitcoinService : ICryptoService
    {
       Task<Transaction> GetInboundTransactionsByRecipientAddressFromEtherscan(string address);
    }
}
