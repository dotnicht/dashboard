using InvestorDashboard.Backend.Database.Models;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class CryptoSettings : ICryptoSettings
    {
        public Currency Currency { get; set; }
        public int Confirmations { get; set; }
        public bool IsDisabled { get; set; }
        public bool ImportDisabledAdressesTransactions { get; set; }
    }
}
