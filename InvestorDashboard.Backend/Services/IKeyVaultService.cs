namespace InvestorDashboard.Backend.Services
{
    public interface IKeyVaultService
    {
        string DatabaseConnectionString { get; }
        string InvestorKeyStoreEncryptionPassword { get; }
        string MasterKeyStoreEncryptionPassword { get; }
    }
}
