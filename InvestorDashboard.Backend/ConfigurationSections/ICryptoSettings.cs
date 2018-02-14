using InvestorDashboard.Backend.Database.Models;
using System;
using System.Numerics;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public interface ICryptoSettings
    {
        Currency Currency { get; set; }
        int Confirmations { get; set; }
        bool IsDisabled { get; set; }
        bool ImportDisabledAddressesTransactions { get; set; }
        Uri NodeAddress { get; set; }
        string InternalTransferUserId { get; set; }
        long StartingBlockIndex { get; set; }
    }
}
