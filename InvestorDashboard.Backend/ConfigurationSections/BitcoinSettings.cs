namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class BitcoinSettings : CryptoSettings
    {
        public bool IsTestNet { get; set; }
        public bool UseSingleTransferTransaction { get; set; }
        public Fee TransactionFee { get; set; }

        public enum Fee
        {
            Fastest, HalfHour, Hour
        }
    }
}
