namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class BitcoinSettings : CryptoSettings
    {
        public string ApiBaseUrl { get; set; }
        public string NetworkType { get; set; }
        public string ApiUri { get; set; }
    }
}
