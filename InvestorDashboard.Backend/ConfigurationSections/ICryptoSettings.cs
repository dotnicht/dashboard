using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public interface ICryptoSettings
    {
        int Confirmations { get; set; }
        bool IsDisabled { get; set; }
        bool ImportDisabledAddressesTransactions { get; set; }
        Uri NodeAddress { get; set; }
    }
}
