using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using Info.Blockchain.API.BlockExplorer;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : CryptoService, IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _bitcoinSettings;
        private readonly IRestService _restService;

        public BitcoinService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IEmailService emailService,
            IAffiliateService affiliatesService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<BitcoinSettings> bitcoinSettings,
            IRestService restService)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, emailService, affiliatesService, mapper, tokenSettings, bitcoinSettings)
        {
            _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        protected override async Task UpdateUserDetailsInternal(string userId)
        {
            var networkType = _bitcoinSettings.Value.NetworkType.Equals(Settings.Value.Currency.ToString(), StringComparison.InvariantCultureIgnoreCase)
                ? Network.Main
                : Network.TestNet;

            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(networkType).ToString();

            await Context.CryptoAddresses.AddAsync(new CryptoAddress
            {
                UserId = userId,
                Currency = Settings.Value.Currency,
                PrivateKey = privateKey.GetEncryptedBitcoinSecret(KeyVaultService.KeyStoreEncryptionPassword, networkType).ToString(),
                Type = CryptoAddressType.Investment,
                Address = address
            });

            Context.SaveChanges();
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            try
            {
                var be = new BlockExplorer();
                var addr = await be.GetBase58AddressAsync(address);
                var mapped = Mapper.Map<List<CryptoTransaction>>(addr.Transactions);

                foreach (var tx in addr.Transactions)
                {
                    mapped.Single(x => x.Hash == tx.Hash).Amount = tx.Outputs.Where(x => x.Address == address).Sum(x => x.Value.GetBtc());
                }

                return mapped;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An error occurred while accessing blockchain.info. Address: { address }.");
                try
                {
                    return await GetFromBlockExplorer(address);
                }
                catch (Exception inner)
                {
                    Logger.LogError(inner, $"An error occurred while accessing block explorer. Address: { address }.");
                    return await GetFromChain(address);
                }
            }
        }

        protected override async Task TransferAssets(CryptoAddress address, string destinationAddress)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<CryptoTransaction>> GetFromBlockExplorer(string address)
        {
            var uri = new Uri(_bitcoinSettings.Value.ApiUri + address);
            var result = await _restService.GetAsync<BlockExplorerResponse>(uri);
            var unmapped = result.Txs.Where(x => x.Confirmations >= _bitcoinSettings.Value.Confirmations);
            var mapped = Mapper.Map<List<CryptoTransaction>>(unmapped);

            foreach (var tx in unmapped)
            {
                mapped.Single(x => x.Hash == tx.Txid).Amount = tx.Vout.Where(x => x.ScriptPubKey.Addresses.Any(y => y == address)).Sum(x => decimal.Parse(x.Value));
            }

            return mapped;
        }

        private async Task<IEnumerable<CryptoTransaction>> GetFromChain(string address)
        {
            var uri = new Uri($"{_bitcoinSettings.Value.ApiBaseUrl}address/{_bitcoinSettings.Value.NetworkType}/{address}");
            var result = await _restService.GetAsync<ChainResponse>(uri);
            return Mapper.Map<List<CryptoTransaction>>(result.Data.Txs.Where(x => x.Confirmations >= _bitcoinSettings.Value.Confirmations));
        }

        internal class ChainResponse
        {
            public string Status { get; set; }
            public ResponseData Data { get; set; }
            public int Code { get; set; }
            public string Message { get; set; }

            public class ResponseData
            {
                public string Network { get; set; }
                public string Address { get; set; }
                public string Balance { get; set; }
                public string Received_value { get; set; }
                public string Pending_value { get; set; }
                public int Total_txs { get; set; }
                public List<Transaction> Txs { get; set; }
            }

            public class Transaction
            {
                public string Txid { get; set; }
                public int? Block_no { get; set; }
                public int Confirmations { get; set; }
                public int Time { get; set; }
                public Outgoing Outgoing { get; set; }
                public Incoming Incoming { get; set; }
            }

            public class Incoming
            {
                public int Output_no { get; set; }
                public string Value { get; set; }
                public Spent Spent { get; set; }
                public List<Input> Inputs { get; set; }
                public int Req_sigs { get; set; }
                public string Script_asm { get; set; }
                public string Script_hex { get; set; }
            }

            public class Outgoing
            {
                public string Value { get; set; }
                public List<Output> Outputs { get; set; }
            }

            public class Output
            {
                public int Output_no { get; set; }
                public string Address { get; set; }
                public string Value { get; set; }
                public Spent Spent { get; set; }
            }

            public class Spent
            {
                public string Txid { get; set; }
                public int Input_no { get; set; }
            }

            public class Input
            {
                public int Input_no { get; set; }
                public string Address { get; set; }
                public ReceivedFrom Received_from { get; set; }
            }

            public class ReceivedFrom
            {
                public string Txid { get; set; }
                public int Output_no { get; set; }
            }
        }

        internal class BlockExplorerResponse
        {
            public int PagesTotal { get; set; }
            public List<Tx> Txs { get; set; }

            public class ScriptSig
            {
                public string Asm { get; set; }
                public string Hex { get; set; }
            }

            public class Vin
            {
                public string Txid { get; set; }
                public int Vout { get; set; }
                public ScriptSig ScriptSig { get; set; }
                public object Sequence { get; set; }
                public int N { get; set; }
                public string Addr { get; set; }
                public int ValueSat { get; set; }
                public double Value { get; set; }
                public object DoubleSpentTxID { get; set; }
            }

            public class ScriptPubKey
            {
                public string Hex { get; set; }
                public string Asm { get; set; }
                public List<string> Addresses { get; set; }
                public string Type { get; set; }
            }

            public class Vout
            {
                public string Value { get; set; }
                public int N { get; set; }
                public ScriptPubKey ScriptPubKey { get; set; }
                public object SpentTxId { get; set; }
                public object SpentIndex { get; set; }
                public object SpentHeight { get; set; }
            }

            public class Tx
            {
                public string Txid { get; set; }
                public int Version { get; set; }
                public int Locktime { get; set; }
                public List<Vin> Vin { get; set; }
                public List<Vout> Vout { get; set; }
                public string Blockhash { get; set; }
                public int Blockheight { get; set; }
                public int Confirmations { get; set; }
                public int Time { get; set; }
                public int Blocktime { get; set; }
                public double ValueOut { get; set; }
                public int Size { get; set; }
                public double ValueIn { get; set; }
                public double Fees { get; set; }
            }
        }
    }
}
