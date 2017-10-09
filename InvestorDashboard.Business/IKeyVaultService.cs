using System.Threading.Tasks;

namespace InvestorDashboard.Business
{
    public interface IKeyVaultService
    {
        string ConnectionString { get; }
        Task Initialize();
    }
}
