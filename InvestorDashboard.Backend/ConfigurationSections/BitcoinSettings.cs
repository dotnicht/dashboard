namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class BitcoinSettings : CryptoSettings
    {
        public bool IsTestNet { get; set; }
        public string LocalBlockchainFilesPath { get; set; }
    }
}
