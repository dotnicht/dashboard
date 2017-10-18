using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService
    {
        Task RefreshInboundTransactions();
    }
}
