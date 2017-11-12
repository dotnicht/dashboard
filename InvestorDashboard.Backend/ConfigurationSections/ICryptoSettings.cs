using System;
using System.Collections.Generic;
using System.Text;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public interface ICryptoSettings
    {
        int Confirmations { get; set; }
        bool IsDisabled { get; set; }
    }
}
