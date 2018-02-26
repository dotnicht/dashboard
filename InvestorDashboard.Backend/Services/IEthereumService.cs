using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IEthereumService : ICryptoService
    {
        Task RefreshOutboundTransactions();
    }
}
