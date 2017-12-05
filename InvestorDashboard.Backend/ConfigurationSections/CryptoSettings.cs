using InvestorDashboard.Backend.Models;
using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class CryptoSettings : ICryptoSettings
    {
        public Currency Currency { get; set; }
        public int Confirmations { get; set; }
        public bool IsDisabled { get; set; }
        public bool ImportDisabledAddressesTransactions { get; set; }
        public Uri NodeAddress { get; set; }
        public string InternalTransferUserId { get; set; }
    }
}
