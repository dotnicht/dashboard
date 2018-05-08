using AutoMapper;
using Info.Blockchain.API.BlockExplorer;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.Protocol;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class BitcoinService : CryptoService, IBitcoinService
    {
        private static IOptions<BitcoinSettings> _bitcoinSettings;
        private static Lazy<ConcurrentChain> _chain = new Lazy<ConcurrentChain>(() => _node.Value.GetChain(), LazyThreadSafetyMode.ExecutionAndPublication);
        private static Lazy<Node> _node = new Lazy<Node>(GetNode, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Network Network
        {
            get
            {
                return _bitcoinSettings.Value.IsTestNet
                    ? Network.TestNet
                    : Network.Main;
            }
        }

        public BitcoinService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IRestService restService,
            ICalculationService calculationService,
            ITokenService tokenService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<BitcoinSettings> bitcoinSettings)
            : base(serviceProvider, loggerFactory, exchangeRateService, keyVaultService, resourceService, restService, calculationService, tokenService, mapper, tokenSettings, bitcoinSettings)
        {
            _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));
        }

        protected override (string Address, string PrivateKey) GenerateKeys(string password = null)
        {
            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(Network).ToString();
            var encrypted = privateKey.GetEncryptedBitcoinSecret(password ?? KeyVaultService.InvestorKeyStoreEncryptionPassword, Network).ToString();
            return (Address: address, PrivateKey: encrypted);
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var be = new BlockExplorer();
            var addr = await be.GetBase58AddressAsync(address);

            var mapped = Mapper.Map<List<CryptoTransaction>>(addr.Transactions);

            foreach (var tx in addr.Transactions)
            {
                var result = mapped.Single(x => x.Hash == tx.Hash);

                if (tx.Inputs.All(x => x.PreviousOutput.Address == address))
                {
                    result.Amount = tx.Outputs.Where(x => x.Address != address).Sum(x => x.Value.Satoshis).ToString();
                    result.Direction = CryptoTransactionDirection.Internal;
                }
                else
                {
                    result.Amount = tx.Outputs.Where(x => x.Address == address).Sum(x => x.Value.Satoshis).ToString();
                    result.Direction = CryptoTransactionDirection.Inbound;
                }
            }

            return mapped;
        }

        protected override async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, BigInteger? amount = null)
        {
            // TODO: implement custom amount transfer.

            var secret = new BitcoinEncryptedSecretNoEC(address.PrivateKey, Network).GetSecret(KeyVaultService.InvestorKeyStoreEncryptionPassword);
            var response = await RestService.GetAsync<EarnResponse>(new Uri("https://bitcoinfees.earn.com/api/v1/fees/recommended"));

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
                ScriptPubKey = BitcoinAddress.Create(destinationAddress, Network).ScriptPubKey
            });

            var fee = Money.Satoshis(response.FastestFee * transaction.GetVirtualSize());
            var value = Money.Coins(balance);

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                transaction.Outputs.Single().Value = adjustedAmount;
                transaction.Sign(secret, false);

                Logger.LogDebug($"Publishing transaction {transaction.ToHex()}");

                _node.Value.SendMessage(new InvPayload(transaction));
                _node.Value.SendMessage(new TxPayload(transaction));
                await Task.Delay(TimeSpan.FromSeconds(5));

                return (Hash: transaction.GetHash().ToString(), AdjustedAmount: adjustedAmount.Satoshi, Success: true);
            }

            if (value > 0)
            {
                Logger.LogWarning($"Transaction publish failed. Address: {address.Address}. Value: {value}. Fee: {fee}.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
        }

        protected override Task<long> GetCurrentBlockIndex()
        {
            var index = _chain.Value.ToEnumerable(false).Last().Height;
            return Task.FromResult((long)index);
        }

        protected override async Task ProccessBlock(long index, IEnumerable<CryptoAddress> addresses)
        {
            var header = _chain.Value.GetBlock((int)index);
            var block = _node.Value.GetBlocks(new[] { header.Header.GetHash() }).Single();

            foreach (var tx in block.Transactions)
            {
                var hash = tx.GetHash().ToString();
                for (var i = 0; i < tx.Outputs.Count; i++)
                {
                    var destination = tx.Outputs[i].ScriptPubKey.GetDestinationAddress(Network)?.ToString();
                    var address = addresses.SingleOrDefault(x => x.Address == destination);
                    if (address != null)
                    {
                        using (var ctx = CreateContext())
                        {
                            if (ctx.CryptoTransactions.SingleOrDefault(x => x.Hash == hash && x.Index == i) == null)
                            {
                                var transaction = new CryptoTransaction
                                {
                                    CryptoAddressId = address.Id,
                                    Direction = CryptoTransactionDirection.Inbound,
                                    Timestamp = header.Header.BlockTime.UtcDateTime,
                                    Hash = hash,
                                    Amount = tx.Outputs[i].Value.Satoshi.ToString(),
                                    Index = i
                                };

                                await ctx.CryptoTransactions.AddAsync(transaction);
                                await ctx.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
        }

        protected override async Task<BigInteger> GetBalance(CryptoAddress address)
        {
            var client = new QBitNinjaClient(Network);
            var script = new BitcoinPubKeyAddress(address.Address, Network);
            var balance = await client.GetBalance(script);
            return new BigInteger(balance.Operations.Sum(x => x.Amount));
            //var balance = await client.GetBalanceSummary(script);
            //return new BigInteger(balance.Confirmed.Amount.Satoshi);
        }

        private static Node GetNode()
        {
            var node = Node.Connect(Network, _bitcoinSettings.Value.NodeAddress.ToString());
            node.VersionHandshake();
            return node;
        }

        private class EarnResponse
        {
            public int FastestFee { get; set; }
            public int HalfHourFee { get; set; }
            public int HourFee { get; set; }
        }
    }
}
