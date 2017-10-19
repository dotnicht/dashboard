using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    public class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;
        private readonly IKeyVaultService _keyVaultService;

        public Currency Currency => Currency.BTC;

        public BitcoinService(IOptions<BitcoinSettings> options, IKeyVaultService keyVaultService)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
        }

        public async Task UpdateUserDetails(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
        }

        public async Task RefreshInboundTransactions()
        {
            
        }

        public async Task<Transaction> GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_options.Value.ApiBaseUrl}/rawaddr/{address}";
            return await RestUtil.Get<Transaction>(uri);
        }

    }

    public class PrevOut
    {
        public bool Spent { get; set; }
        public int Tx_index { get; set; }
        public int Type { get; set; }
        public string Addr { get; set; }
        public long Aalue { get; set; }
        public int N { get; set; }
        public string Script { get; set; }
    }

    public class Input
    {
        public object Sequence { get; set; }
        public string Witness { get; set; }
        public PrevOut Prev_out { get; set; }
        public string Script { get; set; }
    }

    public class Out
    {
        public bool Spent { get; set; }
        public int Tx_index { get; set; }
        public int Type { get; set; }
        public string Addr { get; set; }
        public long Value { get; set; }
        public int N { get; set; }
        public string Script { get; set; }
    }

    public class Tx
    {
        public int Ver { get; set; }
        public IList<Input> Inputs { get; set; }
        public int Weight { get; set; }
        public int Block_height { get; set; }
        public string Relayed_by { get; set; }
        public IList<Out> Out { get; set; }
        public int Lock_time { get; set; }
        public int Result { get; set; }
        public int Size { get; set; }
        public int Time { get; set; }
        public int Tx_index { get; set; }
        public int Vin_sz { get; set; }
        public string Hash { get; set; }
        public int Vout_sz { get; set; }
    }

    public class Transaction
    {
        public string Hash160 { get; set; }
        public string Address { get; set; }
        public int N_tx { get; set; }
        public int Total_received { get; set; }
        public int Total_sent { get; set; }
        public int Final_balance { get; set; }
        public List<Tx> Txs { get; set; }
    }
}
