using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;

        public BitcoinService(IOptions<BitcoinSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IEnumerable<CryptoTransaction> GetInboundTransactionsByRecipientAddress(string address)
        {
            throw new NotImplementedException();
        }

        public void RefreshInboundTransactions()
        {
            throw new NotImplementedException();
        }
    }
}
