using InvestorDashboard.Backend.Models;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services
{
    public interface ICryptoService
    {
        IEnumerable<CryptoTransaction> GetInboundTransactionsByRecipientAddress(string address);
        void RefreshInboundTransactions();
    }
}
