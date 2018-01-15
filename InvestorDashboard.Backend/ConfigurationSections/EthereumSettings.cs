namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class EthereumSettings : CryptoSettings
    {
        public string MasterAccountUserId { get; set; }
        public string ContractAddress { get; set; }
    }
}
