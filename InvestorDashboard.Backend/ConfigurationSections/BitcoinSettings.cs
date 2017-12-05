using NBitcoin;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class BitcoinSettings : CryptoSettings
    {
        public Network NetworkType { get; set; }
    }
}
