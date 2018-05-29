using System.Collections.Generic;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public sealed class KeyVaultSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SecretUri { get; set; }
        public bool ForceDefaults { get; set; }
        public Dictionary<string, string> Defaults { get; set; }
    }
}
