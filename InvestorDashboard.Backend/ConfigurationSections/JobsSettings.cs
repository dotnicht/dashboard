using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class JobsSettings
    {
        public TimeSpan RefreshExchangeRatesPeriod { get; set; }
        public TimeSpan RefreshTokenBalancePeriod { get; set; }
        public TimeSpan RefreshTransactionsPeriod { get; set; }
        public TimeSpan TransferCryptoAssetsPeriod { get; set; }
    }
}
