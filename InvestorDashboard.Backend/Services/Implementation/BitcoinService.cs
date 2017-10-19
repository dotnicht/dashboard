using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    public class BitcoinService : IBitcoinService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<BitcoinSettings> _options;
        private readonly IKeyVaultService _keyVaultService;
        private readonly IExchangeRateService _exchangeRateService;

        public Currency Currency => Currency.BTC;

        public BitcoinService(ApplicationDbContext context, IOptions<BitcoinSettings> options, IKeyVaultService keyVaultService, IExchangeRateService exchangeRateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
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
            //var tokenRate = await _exchangeRateService.GetExchangeRate(Currency.DTT, Currency.USD);
            //var hashes = _context.CryptoTransactions.Select(x => x.Hash).ToHashSet();

            //foreach (var address in _context.CryptoAddresses.Where(x => x.CryptoAccount.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment))
            //{
            //    foreach (var transaction in (await GetInboundTransactionsByRecipientAddressFromEtherscan(address.Address)).Data.Txs)
            //    {
            //        if (!hashes.Contains(transaction.))
            //        {
            //        }
            //    }
            //}
        }

        public async Task<Transaction> GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_options.Value.ApiBaseUrl}address/BTC/{address}";
            return await RestUtil.Get<Transaction>(uri);
        }

    }

    public class Spent
    {
        public string Txid { get; set; }
        public int Input_no { get; set; }
    }

    public class Output
    {
        public int Output_no { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
        public Spent Spent { get; set; }
    }

    public class Outgoing
    {
        public string Value { get; set; }
        public List<Output> Outputs { get; set; }
    }

    public class Spent2
    {
        public string Txid { get; set; }
        public int Input_no { get; set; }
    }

    public class ReceivedFrom
    {
        public string Txid { get; set; }
        public int Output_no { get; set; }
    }

    public class Input
    {
        public int Input_no { get; set; }
        public string Address { get; set; }
        public ReceivedFrom Received_from { get; set; }
    }

    public class Incoming
    {
        public int Output_no { get; set; }
        public string Value { get; set; }
        public Spent2 Spent { get; set; }
        public List<Input> Inputs { get; set; }
        public int Req_sigs { get; set; }
        public string Script_asm { get; set; }
        public string Script_hex { get; set; }
    }

    public class Tx
    {
        public string Txid { get; set; }
        public int Block_no { get; set; }
        public int Confirmations { get; set; }
        public int Time { get; set; }
        public Outgoing Outgoing { get; set; }
        public Incoming Incoming { get; set; }
    }

    public class Data
    {
        public string Network { get; set; }
        public string Address { get; set; }
        public string Balance { get; set; }
        public string Received_value { get; set; }
        public string Pending_value { get; set; }
        public int Total_txs { get; set; }
        public List<Tx> Txs { get; set; }
    }

    public class Transaction
    {
        public string Status { get; set; }
        public Data Data { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
