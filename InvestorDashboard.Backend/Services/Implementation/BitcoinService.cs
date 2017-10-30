using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : CryptoService, IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _bitcoinSettings;

        public override Currency Currency => Currency.BTC;

        public BitcoinService(ApplicationDbContext context, IExchangeRateService exchangeRateService, IKeyVaultService keyVaultService, IMapper mapper, IOptions<TokenSettings> tokenSettings, IOptions<BitcoinSettings> bitcoinSettings)
            : base(context, exchangeRateService, keyVaultService, mapper, tokenSettings)
            => _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));

        protected override async Task UpdateUserDetailsInternal(string userId)
        {
            var networkType = _bitcoinSettings.Value.NetworkType.Equals("BTC", StringComparison.InvariantCultureIgnoreCase)
                ? Network.Main
                : Network.TestNet;

            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(networkType).ToString();

            await Context.CryptoAddresses.AddAsync(new CryptoAddress
            {
                UserId = userId,
                Currency = Currency,
                PrivateKey = privateKey.ToString(networkType), // TODO: encrypt private key with password.
                Type = CryptoAddressType.Investment,
                Address = address
            });

            Context.SaveChanges();
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockChain(string address)
        {
            var uri = $"{_bitcoinSettings.Value.ApiBaseUrl}address/{_bitcoinSettings.Value.NetworkType}/{address}";
            var result = await RestUtil.Get<Transaction>(uri);
            return Mapper.Map<List<CryptoTransaction>>(result.Data.Txs);
        }

        internal class Spent
        {
            public string Txid { get; set; }
            public int Input_no { get; set; }
        }

        internal class Output
        {
            public int Output_no { get; set; }
            public string Address { get; set; }
            public string Value { get; set; }
            public Spent Spent { get; set; }
        }

        internal class Outgoing
        {
            public string Value { get; set; }
            public List<Output> Outputs { get; set; }
        }

        internal class Spent2
        {
            public string Txid { get; set; }
            public int Input_no { get; set; }
        }

        internal class ReceivedFrom
        {
            public string Txid { get; set; }
            public int Output_no { get; set; }
        }

        internal class Input
        {
            public int Input_no { get; set; }
            public string Address { get; set; }
            public ReceivedFrom Received_from { get; set; }
        }

        internal class Incoming
        {
            public int Output_no { get; set; }
            public string Value { get; set; }
            public Spent2 Spent { get; set; }
            public List<Input> Inputs { get; set; }
            public int Req_sigs { get; set; }
            public string Script_asm { get; set; }
            public string Script_hex { get; set; }
        }

        internal class Tx
        {
            public string Txid { get; set; }
            public int? Block_no { get; set; }
            public int Confirmations { get; set; }
            public int Time { get; set; }
            public Outgoing Outgoing { get; set; }
            public Incoming Incoming { get; set; }
        }

        internal class Data
        {
            public string Network { get; set; }
            public string Address { get; set; }
            public string Balance { get; set; }
            public string Received_value { get; set; }
            public string Pending_value { get; set; }
            public int Total_txs { get; set; }
            public List<Tx> Txs { get; set; }
        }

        internal class Transaction
        {
            public string Status { get; set; }
            public Data Data { get; set; }
            public int Code { get; set; }
            public string Message { get; set; }
        }
    }
}
