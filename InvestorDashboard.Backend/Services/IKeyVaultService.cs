using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IKeyVaultService
    {
        string DatabaseConnectionString { get; }
        string KeyStoreEncryptionPassword { get; }
    }
}
