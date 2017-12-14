using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class JobsSettings
    {
        public TimeSpan RefreshExchangeRatesPeriod { get; set; }
        public TimeSpan RefreshTokenBalancePeriod { get; set; }
        public TimeSpan RefreshTransactionsPeriod { get; set; }
        public TimeSpan RefreshDashboardHistoryPeriod { get; set; }
        public TimeSpan TransferAvailableAssetsPeriod { get; set; }
        public TimeSpan SynchronizeExternalInvestorsDataPeriod { get; set; }
        public TimeSpan SynchronizeInternalUsersDataPeriod { get; set; }
        public TimeSpan NotifyAffilicatesTransactionsPeriod { get; set; }
        public TimeSpan NotifyDashboardHistoryPeriod { get; set; }
    }
}
