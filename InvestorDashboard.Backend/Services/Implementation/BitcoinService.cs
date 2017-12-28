﻿using AutoMapper;
using Info.Blockchain.API.BlockExplorer;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : CryptoService, IBitcoinService
    {
        private readonly IOptions<BitcoinSettings> _bitcoinSettings;
        private readonly IRestService _restService;

        protected Network Network
        {
            get
            {
                return _bitcoinSettings.Value.IsTestNet 
                    ? Network.TestNet 
                    : Network.Main;
            }
        }

        public BitcoinService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            ICsvService csvService,
            IMessageService messageService,
            IDashboardHistoryService dashboardHistoryService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<BitcoinSettings> bitcoinSettings,
            IRestService restService)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, csvService, messageService, dashboardHistoryService, mapper, tokenSettings, bitcoinSettings)
        {
            _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        protected override (string Address, string PrivateKey) GenerateKeys()
        {
            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(Network).ToString();
            var encrypted = privateKey.GetEncryptedBitcoinSecret(KeyVaultService.KeyStoreEncryptionPassword, Network).ToString();
            return (Address: address, PrivateKey: encrypted);
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var sources = new Dictionary<string, Func<string, Task<IEnumerable<CryptoTransaction>>>>
            {
                { "blockchain.info", GetFromBlockchain },
                { "blockexplorer.com", GetFromBlockExplorer },
                { "chain.so", GetFromChain }
            };

            foreach (var source in sources)
            {
                try
                {
                    return await source.Value(address);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while getting transaction info from { source.Key }.");
                }
            }

            throw new InvalidOperationException($"All sources failed to retrieve transaction info.");
        }

        protected override async Task<(string Hash, decimal AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, decimal? amount = null)
        {
            // TODO: implement custom amount transfer.

            var secret = new BitcoinEncryptedSecretNoEC(address.PrivateKey, Network).GetSecret(KeyVaultService.KeyStoreEncryptionPassword);
            var response = _restService.Get<EarnResponse>(new Uri("https://bitcoinfees.earn.com/api/v1/fees/recommended"));

            var balance = 0m;
            var transaction = new Transaction();

            foreach (var tx in (await new BlockExplorer().GetBase58AddressAsync(address.Address)).Transactions)
            {
                for (var i = 0; i < tx.Outputs.Count; i++)
                {
                    if (tx.Outputs[i].Address == address.Address && !tx.Outputs[i].Spent)
                    {
                        balance += tx.Outputs[i].Value.GetBtc();
                        transaction.AddInput(new TxIn
                        {
                            PrevOut = new OutPoint(new uint256(tx.Hash), i),
                            ScriptSig = secret.GetAddress().ScriptPubKey
                        });
                    }
                }
            }

            transaction.AddOutput(new TxOut
            {
                ScriptPubKey = BitcoinAddress.Create(destinationAddress).ScriptPubKey
            });

            var fee = Money.Satoshis(response.FastestFee * transaction.GetVirtualSize());
            var value = Money.Coins(balance);

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                transaction.Outputs.Single().Value = adjustedAmount;
                transaction.Sign(secret, false);

                Logger.LogDebug($"Publishing transaction { transaction.ToHex() }");

                var node = Node.Connect(Network, _bitcoinSettings.Value.NodeAddress.ToString());
                node.VersionHandshake();
                node.SendMessage(new InvPayload(transaction));
                node.SendMessage(new TxPayload(transaction));
                await Task.Delay(TimeSpan.FromSeconds(5));
                node.Disconnect();

                return (Hash: transaction.GetHash().ToString(), AdjustedAmount: adjustedAmount.ToDecimal(MoneyUnit.BTC), Success: true);
            }

            if (value > 0)
            {
                Logger.LogError($"Transaction publish failed. Address: { address.Address }. Value: { value }. Fee: { fee }.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
        }

        private async Task<IEnumerable<CryptoTransaction>> GetFromBlockchain(string address)
        {
            var be = new BlockExplorer();
            var addr = await be.GetBase58AddressAsync(address);

            var mapped = Mapper.Map<List<CryptoTransaction>>(addr.Transactions);

            foreach (var tx in addr.Transactions)
            {
                var result = mapped.Single(x => x.Hash == tx.Hash);

                if (tx.Inputs.All(x => x.PreviousOutput.Address == address))
                {
                    result.Amount = tx.Outputs.Where(x => x.Address != address).Sum(x => x.Value.GetBtc());
                    result.Direction = CryptoTransactionDirection.Internal;
                }
                else
                {
                    result.Amount = tx.Outputs.Where(x => x.Address == address).Sum(x => x.Value.GetBtc());
                    result.Direction = CryptoTransactionDirection.Inbound;
                }
            }

            return mapped;
        }

        private async Task<IEnumerable<CryptoTransaction>> GetFromBlockExplorer(string address)
        {
            var uri = new Uri($"https://blockexplorer.com/api/txs/?address={ address }");
            var result = await _restService.GetAsync<BlockExplorerResponse>(uri);
            var unmapped = result.Txs.Where(x => x.Confirmations >= _bitcoinSettings.Value.Confirmations);
            var mapped = Mapper.Map<List<CryptoTransaction>>(unmapped);

            foreach (var tx in unmapped)
            {
                var transaction = mapped.Single(x => x.Hash == tx.Txid);

                if (tx.Vout.Any(x => x.ScriptPubKey.Addresses.Any(y => y == address)))
                {
                    transaction.Amount = tx.Vout.Where(x => x.ScriptPubKey.Addresses.Any(y => y == address)).Sum(x => decimal.Parse(x.Value));
                    transaction.Direction = CryptoTransactionDirection.Inbound;
                }
                else
                {
                    transaction.Amount = tx.Vout.Where(x => x.ScriptPubKey.Addresses.Any(y => y != address)).Sum(x => decimal.Parse(x.Value));
                    transaction.Direction = CryptoTransactionDirection.Internal;
                }
            }

            return mapped;
        }

        private async Task<IEnumerable<CryptoTransaction>> GetFromChain(string address)
        {
            var uri = new Uri($"https://chain.so/api/v2/address/{ (_bitcoinSettings.Value.IsTestNet ? "BTCTEST" : "BTC") }/{address}");
            var result = await _restService.GetAsync<ChainResponse>(uri);
            var unmapped = result.Data.Txs.Where(x => x.Confirmations >= _bitcoinSettings.Value.Confirmations);
            var mapped = Mapper.Map<List<CryptoTransaction>>(unmapped);

            foreach (var tx in unmapped)
            {
                var transaction = mapped.Single(x => x.Hash == tx.Txid);

                if (tx.Outgoing.Outputs.Any(x => x.Address == address))
                {
                    transaction.Amount = tx.Outgoing.Outputs.Where(x => x.Address == address).Sum(x => decimal.Parse(x.Value));
                    transaction.Direction = CryptoTransactionDirection.Inbound;
                }
                else
                {
                    transaction.Amount = tx.Outgoing.Outputs.Where(x => x.Address != address).Sum(x => decimal.Parse(x.Value));
                    transaction.Direction = CryptoTransactionDirection.Internal;
                }
            }

            return mapped;
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

        private class EarnResponse
        {
            public int FastestFee { get; set; }
            public int HalfHourFee { get; set; }
            public int HourFee { get; set; }
        }
    }
}
