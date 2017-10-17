using System;
using System.Collections.Generic;
using System.Text;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class EthereumSettings
    {
        public string EtherchainApiUri { get; set; }
        public string EtherscanApiUri { get; set; }
        public string EtherscanApiKey { get; set; }
    }
}
