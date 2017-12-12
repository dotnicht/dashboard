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
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : CryptoService, IEthereumService
    {
        private readonly IOptions<EthereumSettings> _ethereumSettings;
        private readonly IRestService _restService;

        public EthereumService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            ICsvService csvService,
            IMessageService messageService,
            IAffiliateService affiliatesService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<EthereumSettings> ethereumSettings,
            IRestService restService)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, csvService, messageService, affiliatesService, mapper, tokenSettings, ethereumSettings)
        {
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        protected override (string Address, string PrivateKey) GenerateKeys()
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
                var privateKey = service.EncryptAndGenerateKeyStoreAsJson(KeyVaultService.KeyStoreEncryptionPassword, bytes, address);
                return (Address: address, PrivateKey: privateKey);
            });
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var uri = new Uri($"http://api.etherscan.io/api?module=account&action=txlist&address={ address }&startblock=0&endblock=99999999&sort=asc&apikey=QJZXTMH6PUTG4S3IA4H5URIIXT9TYUGI7P");
            var result = await _restService.GetAsync<EtherscanResponse>(uri);

            var confirmed = result.Result
                .Where(x => int.Parse(x.Confirmations) >= _ethereumSettings.Value.Confirmations)
                .ToArray();

            return Mapper.Map<List<CryptoTransaction>>(confirmed);
        }

        protected override async Task<(string Hash, decimal AdjustedAmount)> PublishTransactionInternal(CryptoAddress address, string destinationAddress, decimal? amount = null)
        {
            var web3 = new Web3(Account.LoadFromKeyStore(address.PrivateKey, KeyVaultService.KeyStoreEncryptionPassword), Settings.Value.NodeAddress.ToString());

            var value = amount == null
                ? await web3.Eth.GetBalance.SendRequestAsync(address.Address)
                : new HexBigInteger(UnitConversion.Convert.ToWei(amount.Value));

            web3.TransactionManager.DefaultGasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var adjustedAmount = value - web3.TransactionManager.DefaultGas * web3.TransactionManager.DefaultGasPrice;
            var hash = await web3.TransactionManager.SendTransactionAsync(address.Address, destinationAddress, new HexBigInteger(adjustedAmount));

            return (Hash: hash, AdjustedAmount: UnitConversion.Convert.FromWei(adjustedAmount));
        }

        internal class EtherscanResponse
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
    }
}
