using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _options;

        public BitcoinService(IOptions<BitcoinSettings> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task RefreshInboundTransactions()
        {
            throw new NotImplementedException();
        }

        private async Task<Transaction> GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_options.Value.ApiBaseUrl}/rawaddr/{address}";
            var result = await RestUtil.Get<Transaction>(uri);
            return result;
        }

    }

    internal class PrevOut
    {
        public bool Spent { get; set; }
        public int Tx_index { get; set; }
        public int Type { get; set; }
        public string Addr { get; set; }
        public long Aalue { get; set; }
        public int N { get; set; }
        public string Script { get; set; }
    }

    internal class Input
    {
        public object Sequence { get; set; }
        public string Witness { get; set; }
        public PrevOut Prev_out { get; set; }
        public string Script { get; set; }
    }

    internal class Out
    {
        public bool Spent { get; set; }
        public int Tx_index { get; set; }
        public int Type { get; set; }
        public string Addr { get; set; }
        public long Value { get; set; }
        public int N { get; set; }
        public string Script { get; set; }
    }

    internal class Tx
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

    internal class Transaction
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
