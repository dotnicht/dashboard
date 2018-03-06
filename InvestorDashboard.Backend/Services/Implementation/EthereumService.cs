using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Util;
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
        private readonly ITokenService _tokenService;
        private readonly IOptions<EthereumSettings> _ethereumSettings;
        private readonly IRestService _restService;

        public EthereumService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IMessageService messageService,
            IDashboardHistoryService dashboardHistoryService,
            ITokenService tokenService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<EthereumSettings> ethereumSettings,
            IRestService restService)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, resourceService, messageService, dashboardHistoryService, tokenService, mapper, tokenSettings, ethereumSettings)
        {
            _tokenService = tokenService;
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        public async Task RefreshOutboundTransactions()
        {
            var transactions = Context.CryptoTransactions
                .Where(
                    x => x.Direction == CryptoTransactionDirection.Outbound
                    && x.IsFailed == null
                    && x.ExternalId == null
                    && x.CryptoAddress.Address == null
                    && x.CryptoAddress.Currency == Currency.Token
                    && x.CryptoAddress.Type == CryptoAddressType.Transfer)
                .ToArray();

            foreach (var tx in transactions)
            {
                var web3 = new Web3(_ethereumSettings.Value.NodeAddress.ToString());
                var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx.Hash);

                if (receipt != null)
                {
                    tx.IsFailed = !Convert.ToBoolean(receipt.Status.Value);
                    await Context.SaveChangesAsync();
                }
            }
        }

        public override async Task SynchronizeRawTransactions()
        {
            var index = new BigInteger(_ethereumSettings.Value.StartingBlockIndex);
            var web3 = new Web3(_ethereumSettings.Value.NodeAddress.ToString());

            while (true)
            {
                var current = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                while (index <= current.Value)
                {
                    if (!Context.RawBlocks.Any(x => x.Index == index && x.Currency == Currency.ETH))
                    {
                        var source = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(index));

                        var block = new RawBlock
                        {
                            Hash = source.BlockHash,
                            Index = (long)source.Number.Value,
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(source.Timestamp.Value.ToString())).UtcDateTime
                        };

                        foreach (var tx in source.Transactions)
                        {
                            var transaction = new RawTransaction
                            {
                                Hash = tx.TransactionHash,
                                Index = (long)tx.TransactionIndex.Value
                            };

                            transaction.Parts.Add(new RawPart
                            {
                                Type = RawPartType.Input,
                                Address = tx.From,
                                Value = tx.Value.Value.ToString()
                            });

                            transaction.Parts.Add(new RawPart
                            {
                                Type = RawPartType.Output,
                                Address = tx.To,
                                Value = tx.Value.Value.ToString()
                            });

                            block.Transactions.Add(transaction);
                        }

                        await Context.RawBlocks.AddAsync(block);
                        await Context.SaveChangesAsync();
                    }

                    index++;
                }

                await Task.Delay(_ethereumSettings.Value.IdleModeRefreshPeriod);
            }
        }

        protected override (string Address, string PrivateKey) GenerateKeys(string password = null)
        {
            var policy = Policy
                .Handle<ArgumentException>()
                .Retry(10, (e, i) => Logger.LogError(e, $"Key generation failed. Retry attempt: {i}."));

            return policy.Execute(() =>
            {
                var ecKey = EthECKey.GenerateKey();
                var address = ecKey.GetPublicAddress();
                var bytes = ecKey.GetPrivateKeyAsBytes();
                var service = new KeyStorePbkdf2Service();
                var privateKey = service.EncryptAndGenerateKeyStoreAsJson(password ?? KeyVaultService.InvestorKeyStoreEncryptionPassword, bytes, address);
                return (Address: address, PrivateKey: privateKey);
            });
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var transactions = new List<CryptoTransaction>();

            foreach (var action in new[] { "txlist", "txlistinternal" })
            {
                var uri = new Uri($"http://api.etherscan.io/api?module=account&action={action}&address={address}&startblock=0&endblock=99999999&sort=asc&apikey=QJZXTMH6PUTG4S3IA4H5URIIXT9TYUGI7P");

                try
                {
                    var result = await _restService.GetAsync<EtherscanAccountResponse>(uri);

                    var confirmed = result.Result
                        .Where(x => string.IsNullOrWhiteSpace(x.Confirmations) || int.Parse(x.Confirmations) >= _ethereumSettings.Value.Confirmations)
                        .ToArray();

                    var mapped = Mapper.Map<CryptoTransaction[]>(confirmed);

                    foreach (var tx in mapped)
                    {
                        // TODO: adjust direction to include outbound transactions.
                        var source = confirmed.Single(x => string.Equals(x.Hash, tx.Hash, StringComparison.InvariantCultureIgnoreCase));
                        tx.Direction = string.Equals(source.To, address, StringComparison.InvariantCultureIgnoreCase)
                            ? CryptoTransactionDirection.Inbound
                            : string.Equals(source.From, address, StringComparison.InvariantCultureIgnoreCase)
                                ? CryptoTransactionDirection.Internal
                                : throw new InvalidOperationException($"Unable to determine transaction direction. Hash: {tx.Hash}.");
                    }

                    transactions.AddRange(mapped);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An error occurred while accessing URI {uri}.");
                    throw;
                }
            }

            return transactions;
        }

        protected override async Task<(string Hash, decimal AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, decimal? amount = null)
        {
            var web3 = new Web3(Account.LoadFromKeyStore(address.PrivateKey, KeyVaultService.InvestorKeyStoreEncryptionPassword), Settings.Value.NodeAddress.ToString());

            var value = amount == null
                ? await web3.Eth.GetBalance.SendRequestAsync(address.Address)
                : new HexBigInteger(UnitConversion.Convert.ToWei(amount.Value));

            web3.TransactionManager.DefaultGasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var fee = web3.TransactionManager.DefaultGas * web3.TransactionManager.DefaultGasPrice;

            if (value > fee)
            {
                var adjustedAmount = value - fee;
                var hash = await web3.TransactionManager.SendTransactionAsync(address.Address, destinationAddress, new HexBigInteger(adjustedAmount));

                return (Hash: hash, AdjustedAmount: UnitConversion.Convert.FromWei(adjustedAmount), Success: true);
            }

            if (value.Value > 0)
            {
                Logger.LogWarning($"Transaction publish failed. Address: {address.Address}. Value: {value.Value}. Fee: {fee}.");
            }

            return (Hash: null, AdjustedAmount: 0, Success: false);
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
