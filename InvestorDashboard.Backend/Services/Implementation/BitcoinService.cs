using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.Protocol;
using QBitNinja.Client;
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
            IOptions<BitcoinSettings> bitcoinSettings,
            IOptions<ReferralSettings> referralSettings)
            : base(serviceProvider, loggerFactory, exchangeRateService, keyVaultService, resourceService, restService, calculationService, tokenService, mapper, tokenSettings, bitcoinSettings, referralSettings)
        {
            _bitcoinSettings = bitcoinSettings ?? throw new ArgumentNullException(nameof(bitcoinSettings));
        }

        public override async Task TransferAvailableAssets()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            if (_bitcoinSettings.Value.UseSingleTransferTransaction)
            {
                var transaction = new Transaction();
                var client = new QBitNinjaClient(Network);
                var value = Money.Zero;

                var secrets = new List<ISecret>();
                var coins = new List<ICoin>();

                using (var ctx = CreateContext())
                {
                    foreach (var tx in GetTransferTransactions(ctx))
                    {
                        var secret = new BitcoinEncryptedSecretNoEC(tx.CryptoAddress.PrivateKey, Network).GetSecret(KeyVaultService.InvestorKeyStoreEncryptionPassword);
                        var balance = await client.GetBalance(secret, true);

                        secrets.Add(secret);

                        foreach (var coin in balance.Operations.SelectMany(x => x.ReceivedCoins))
                        {
                            transaction.AddInput(new TxIn(coin.Outpoint, secret.GetAddress().ScriptPubKey));
                            value += coin.TxOut.Value;
                            coins.Add(coin);
                        }

                        tx.IsSpent = true;
                    }

                    if (!ReferralSettings.Value.IsDisabled)
                    {
                        foreach (var addr in GetReferralTransferAddresses(ctx))
                        {
                            var amount = Money.Satoshis(addr.Sum(x => long.Parse(x.Amount)) * ReferralSettings.Value.Reward);
                            value -= amount;
                            transaction.AddOutput(amount, BitcoinAddress.Create(addr.Key, Network));
                            addr.ToList().ForEach(x => x.IsReferralPaid = true);
                        }
                    }

                    var (hash, adjustedAmount, success) = await AdjustAmountAndPublish(transaction, secrets.ToArray(), coins.ToArray(), value, GetTransferDestinationAddress());

                    if (success)
                    {
                        foreach (var tx in GetTransferTransactions(ctx))
                        {
                            var transfer = new CryptoTransaction
                            {
                                Hash = hash,
                                Amount = adjustedAmount.ToString(),
                                Timestamp = DateTime.UtcNow,
                                Direction = CryptoTransactionDirection.Internal,
                                CryptoAddressId = tx.CryptoAddressId
                            };

                            if (tx.IsReferralPaid)
                            {
                                // TODO: adjust tx value.
                                var referral = new CryptoTransaction
                                {
                                    Hash = hash,
                                    Amount = adjustedAmount.ToString(),
                                    Timestamp = DateTime.UtcNow,
                                    Direction = CryptoTransactionDirection.Referral,
                                    CryptoAddressId = tx.CryptoAddressId
                                };

                                ctx.CryptoTransactions.Add(referral);
                            }

                            ctx.CryptoTransactions.Add(transfer);
                        }

                        await ctx.SaveChangesAsync();
                    }
                }
            }
            else
            {
                await base.TransferAvailableAssets();
            }
        }

        protected override (string Address, string PrivateKey) GenerateKeys(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var privateKey = new Key();
            var address = privateKey.PubKey.GetAddress(Network).ToString();
            var encrypted = privateKey.GetEncryptedBitcoinSecret(password, Network).ToString();
            return (Address: address, PrivateKey: encrypted);
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var client = new QBitNinjaClient(Network);
            var script = new BitcoinPubKeyAddress(address, Network);
            var balance = await client.GetBalance(script, true);

            return balance.Operations
                .Select(
                    x => new CryptoTransaction
                    {
                        Hash = x.TransactionId.ToString(),
                        Timestamp = x.FirstSeen.UtcDateTime,
                        Amount = x.Amount.Satoshi.ToString(),
                        BlockIndex = x.Height,
                        Direction = CryptoTransactionDirection.Inbound
                    })
                .ToArray();
        }

        protected override async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, BigInteger? amount = null)
        {
            var transaction = new Transaction();
            var secret = new BitcoinEncryptedSecretNoEC(address.PrivateKey, Network).GetSecret(KeyVaultService.InvestorKeyStoreEncryptionPassword);
            var client = new QBitNinjaClient(Network);
            var result = await client.GetBalance(secret, true);

            var coins = result.Operations
                .SelectMany(x => x.ReceivedCoins)
                .ToArray();

            var balance = new BigInteger(coins.Sum(x => x.TxOut.Value));
            amount = amount ?? balance;

            if (amount > balance)
            {
                throw new InvalidOperationException($"Low balance. Requested: {amount}. Actual: {balance}.");
            }

            var money = Money.Satoshis(long.Parse(amount.Value.ToString()));
            var value = Money.Zero;

            foreach (var coin in coins)
            {
                transaction.AddInput(new TxIn(coin.Outpoint, secret.GetAddress().ScriptPubKey));
                value += coin.TxOut.Value;
                if (value >= amount)
                {
                    break;
                }
            }

            var change = value - money;

            if (change > 0)
            {
                transaction.AddOutput(new TxOut
                {
                    ScriptPubKey = secret.GetAddress().ScriptPubKey,
                    Value = change
                });
            }

            return await AdjustAmountAndPublish(transaction, new[] { secret }, coins, value, destinationAddress);
        }

        protected override Task<long> GetCurrentBlockIndex()
        {
            var index = _chain.Value.ToEnumerable(false).Last().Height;
            return Task.FromResult((long)index);
        }

        protected override Task ProccessBlock(long index, IEnumerable<CryptoAddress> addresses)
        {
            if (addresses == null)
            {
                throw new ArgumentNullException(nameof(addresses));
            }

            var header = _chain.Value.GetBlock((int)index);
            var block = _node.Value.GetBlocks(new[] { header.Header.GetHash() }).Single();

            Parallel.ForEach(addresses, x =>
            {
                var address = new BitcoinPubKeyAddress(x.Address, Network);
                var transactions = block.Transactions.Where(y => y.Outputs.Any(z => z.ScriptPubKey.GetDestinationAddress(Network) == address));

                using (var ctx = CreateContext())
                {
                    foreach (var tx in transactions)
                    {
                        var hash = tx.GetHash().ToString();
                        if (!ctx.CryptoTransactions.Any(y => y.Hash == hash && y.Direction == CryptoTransactionDirection.Inbound && y.CryptoAddressId == x.Id))
                        {
                            var transaction = new CryptoTransaction
                            {
                                CryptoAddressId = x.Id,
                                Direction = CryptoTransactionDirection.Inbound,
                                Timestamp = header.Header.BlockTime.UtcDateTime,
                                BlockIndex = header.Height,
                                Hash = hash,
                                Amount = tx.Outputs
                                    .Where(y => y.ScriptPubKey.GetDestinationAddress(Network) == address)
                                    .Sum(y => y.Value)
                                    .ToString()
                            };

                            ctx.CryptoTransactions.Add(transaction);
                            ctx.SaveChanges();
                        }
                    }
                }
            });

            return Task.CompletedTask;
        }

        protected override async Task<BigInteger> GetBalance(CryptoAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var client = new QBitNinjaClient(Network);
            var script = new BitcoinPubKeyAddress(address.Address, Network);
            //var balance = await client.GetBalance(script);
            //return new BigInteger(balance.Operations.Sum(x => x.Amount));
            var balance = await client.GetBalanceSummary(script);
            return new BigInteger(balance.Confirmed.Amount.Satoshi);
        }

        private static Node GetNode()
        {
            var node = Node.Connect(Network, _bitcoinSettings.Value.NodeAddress.ToString());
            node.VersionHandshake();
            return node;
        }

        private async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> AdjustAmountAndPublish(Transaction transaction, ISecret[] secrets, ICoin[] coins, Money value, string address)
        {
            var destination = new TxOut
            {
                ScriptPubKey = BitcoinAddress.Create(address, Network).ScriptPubKey
            };

            transaction.AddOutput(destination);

            var response = await RestService.GetAsync<EarnResponse>(new Uri("https://bitcoinfees.earn.com/api/v1/fees/recommended"));
            var fee = Money.Satoshis(response.FastestFee * transaction.GetVirtualSize());

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                destination.Value = adjustedAmount;
                transaction.Sign(secrets, coins);

                Logger.LogDebug($"Publishing transaction {transaction.ToHex()}");

                _node.Value.SendMessage(new InvPayload(transaction));
                _node.Value.SendMessage(new TxPayload(transaction));

                await Task.Delay(TimeSpan.FromSeconds(5));

                return (Hash: transaction.GetHash().ToString(), AdjustedAmount: adjustedAmount.Satoshi, Success: true);
            }

            if (value > 0)
            {
                Logger.LogWarning($"Transaction publish failed. Value: {value}. Fee: {fee}.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
        }

        private class EarnResponse
        {
            public int FastestFee { get; set; }
            public int HalfHourFee { get; set; }
            public int HourFee { get; set; }
        }
    }
}
