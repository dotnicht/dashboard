using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class JobsSettings
    {
        public TimeSpan RefreshExchangeRatesPeriod { get; set; }
        public TimeSpan RefreshTokenBalancePeriod { get; set; }
        public TimeSpan RefreshTransactionsPeriod { get; set; }
        public TimeSpan TransferCryptoAssetsPeriod { get; set; }
        public TimeSpan InvestorsActivationPeriod { get; set; }
        public TimeSpan InvestorsLoadPeriod { get; set; }
        public TimeSpan InvestorsClearPeriod { get; set; }
        public TimeSpan AffilicatesSyncPeriod { get; set; }
        public TimeSpan UpdateUserDetailsPeriod { get; set; }
    }
}
