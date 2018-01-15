﻿using System;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class EthereumSettings : CryptoSettings
    {
        public string MasterAccountUserId { get; set; }
        public string ContractAddress { get; set; }
        public int DefaultGas { get; set; }
        public TimeSpan AccountUnlockWindow { get; set; }
    }
}
