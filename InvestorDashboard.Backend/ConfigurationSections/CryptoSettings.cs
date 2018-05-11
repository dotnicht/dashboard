using InvestorDashboard.Backend.Database.Models;
using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class CryptoSettings
    {
        public Currency Currency { get; set; }
        public int Confirmations { get; set; }
        public bool IsDisabled { get; set; }
        public bool SkipAddressCreationOnDisabled { get; set; }
        public bool ImportDisabledAddressesTransactions { get; set; }
        public Uri NodeAddress { get; set; }
        public string InternalTransferUserId { get; set; }
        public byte Denomination { get; set; }
        public bool UseDirectBlockAccess { get; set; }
        public TimeSpan? LegacyTransactionRefreshTimeout { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
    }
}
