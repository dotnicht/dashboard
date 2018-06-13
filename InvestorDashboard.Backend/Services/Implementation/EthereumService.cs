using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : CryptoService, IEthereumService
    {
        private readonly IOptions<EthereumSettings> _ethereumSettings;

        private Web3 _web3;
        private Web3 Web3 { get => _web3 ?? (_web3 = new Web3(_ethereumSettings.Value.NodeAddress.ToString())); }

        public EthereumService(
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
            IOptions<EthereumSettings> ethereumSettings,
            IOptions<ReferralSettings> referralSettings)
            : base(serviceProvider, loggerFactory, exchangeRateService, keyVaultService, resourceService, restService, calculationService, tokenService, mapper, tokenSettings, ethereumSettings, referralSettings)
        {
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
        }

        protected override (string Address, string PrivateKey) GenerateKeys(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var policy = Policy
                .Handle<ArgumentException>()
                .Retry(10, (e, i) => Logger.LogError(e, $"Key generation failed. Retry attempt: {i}."));

            return policy.Execute(() =>
            {
                var ecKey = EthECKey.GenerateKey();
                var address = ecKey.GetPublicAddress();
                var bytes = ecKey.GetPrivateKeyAsBytes();
                var service = new KeyStorePbkdf2Service();
                var privateKey = service.EncryptAndGenerateKeyStoreAsJson(password, bytes, address);
                return (Address: address, PrivateKey: privateKey);
            });
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var transactions = new List<CryptoTransaction>();

            foreach (var action in new[] { "txlist", "txlistinternal" })
            {
                var uri = new Uri($"https://api.etherscan.io/api?module=account&action={action}&address={address}&startblock=0&endblock=99999999&sort=asc&apikey=QJZXTMH6PUTG4S3IA4H5URIIXT9TYUGI7P");

                try
                {
                    var result = await RestService.GetAsync<EtherscanAccountResponse>(uri);

                    if (result == null || result.Result == null)
                    {
                        throw new InvalidOperationException($"An error occurred while processing URL {uri}. Madnatory data missing.");
                    }

                    var confirmed = result.Result
                        .Where(x => string.Equals(address, x.To, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrWhiteSpace(x.Confirmations) || int.Parse(x.Confirmations) >= _ethereumSettings.Value.Confirmations))
                        .ToArray();

                    foreach (var tx in Mapper.Map<CryptoTransaction[]>(confirmed))
                    {
                        tx.Direction = CryptoTransactionDirection.Inbound;
                        transactions.Add(tx);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while accessing URI {uri}.");
                }
            }

            return transactions;
        }

        protected override async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, BigInteger? amount = null)
        {
            var web3 = new Web3(Account.LoadFromKeyStore(address.PrivateKey, KeyVaultService.InvestorKeyStoreEncryptionPassword), Settings.Value.NodeAddress.ToString());

            var value = amount == null
                ? await web3.Eth.GetBalance.SendRequestAsync(address.Address)
                : new HexBigInteger(amount.Value);

            web3.TransactionManager.DefaultGasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var fee = web3.TransactionManager.DefaultGas * web3.TransactionManager.DefaultGasPrice;

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                var hash = await web3.TransactionManager.SendTransactionAsync(address.Address, destinationAddress, new HexBigInteger(adjustedAmount));

                return (Hash: hash, AdjustedAmount: adjustedAmount, Success: true);
            }

            if (value.Value > 0)
            {
                Logger.LogWarning($"Transaction publish failed. Address: {address.Address}. Value: {value.Value}. Fee: {fee}.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
        }

        protected override async Task<long> GetCurrentBlockIndex()
        {
            var result = await Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            return (long)result.Value;
        }

        protected override async Task ProccessBlock(long index, IEnumerable<CryptoAddress> addresses)
        {
            var block = await Web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(index));

            foreach (var tx in block.Transactions)
            {
                var address = addresses.SingleOrDefault(x => x.Address == tx.To);
                if (address != null)
                {
                    using (var ctx = CreateContext())
                    {
                        if (ctx.CryptoTransactions.SingleOrDefault(x => x.Hash == tx.TransactionHash) == null)
                        {
                            var transaction = new CryptoTransaction
                            {
                                CryptoAddressId = address.Id,
                                Direction = CryptoTransactionDirection.Inbound,
                                Timestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value).UtcDateTime,
                                Hash = tx.TransactionHash,
                                Amount = tx.Value.Value.ToString(),
                                BlockIndex = index
                            };

                            ctx.Attach(address);
                            address.LastBlockIndex = index;

                            await ctx.CryptoTransactions.AddAsync(transaction);
                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        protected override async Task<BigInteger> GetBalance(CryptoAddress address)
        {
            var balance = await Web3.Eth.GetBalance.SendRequestAsync(address.Address);
            return balance.Value;
        }

        internal class EtherscanAccountResponse
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public List<Transaction> Result { get; set; }

            public class Transaction
            {
                public string BlockNumber { get; set; }
                public string BlockHash { get; set; }
                public string TimeStamp { get; set; }
                public string Hash { get; set; }
                public string Nonce { get; set; }
                public string TransactionIndex { get; set; }
                public string From { get; set; }
                public string To { get; set; }
                public string Value { get; set; }
                public string Gas { get; set; }
                public string GasPrice { get; set; }
                public string Input { get; set; }
                public string ContractAddress { get; set; }
                public string CumulativeGasUsed { get; set; }
                public string GasUsed { get; set; }
                public string Confirmations { get; set; }
                public string IsError { get; set; }
            }
        }

        private class EtherscanTransactionResponse
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public Transaction Result { get; set; }

            public class Transaction
            {
                public int IsError { get; set; }
                public string ErrDescription { get; set; }
            }
        }
    }
}
