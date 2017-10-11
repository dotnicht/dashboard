using System.Threading.Tasks;

namespace InvestorDashboard.Business.Services
{
    public interface IKeyVaultService
    {
        string DatabaseConnectionString { get; }
        string KeyStoreEncryptionPassword { get; }
        Task Initialize();
    }
}
