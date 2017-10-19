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
            var uri = $"{_options.Value.ApiBaseUrl}address/BTC/{address}";
            return await RestUtil.Get<Transaction>(uri);
        }

    }

    public class ReceivedFrom
    {
        public string txid { get; set; }
        public int output_no { get; set; }
    }

    public class Input
    {
        public int input_no { get; set; }
        public string address { get; set; }
        public ReceivedFrom received_from { get; set; }
    }

    public class Incoming
    {
        public int output_no { get; set; }
        public string value { get; set; }
        public object spent { get; set; }
        public List<Input> inputs { get; set; }
        public int req_sigs { get; set; }
        public string script_asm { get; set; }
        public string script_hex { get; set; }
    }

    public class Tx
    {
        public string txid { get; set; }
        public int block_no { get; set; }
        public int confirmations { get; set; }
        public int time { get; set; }
        public Incoming incoming { get; set; }
    }

    public class Data
    {
        public string network { get; set; }
        public string address { get; set; }
        public string balance { get; set; }
        public string received_value { get; set; }
        public string pending_value { get; set; }
        public int total_txs { get; set; }
        public List<Tx> txs { get; set; }
    }

    public class Transaction
    {
        public string status { get; set; }
        public Data data { get; set; }
        public int code { get; set; }
        public string message { get; set; }
    }
}
