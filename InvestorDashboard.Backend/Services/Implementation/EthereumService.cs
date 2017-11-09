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
using Nethereum.KeyStore;
using Nethereum.Signer;
using Polly;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : CryptoService, IEthereumService
    {
        private readonly IOptions<EthereumSettings> _ethereumSettings;
        private readonly IRestService _restService;

        public override Currency Currency => Currency.ETH;
        public override int Confirmations => _ethereumSettings.Value.Confirmations;

        public EthereumService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IEmailService emailService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<EthereumSettings> ethereumSettings,
            IRestService restService)
            : base(context, loggerFactory, exchangeRateService, keyVaultService, emailService, mapper, tokenSettings)
        {
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        protected override async Task UpdateUserDetailsInternal(string userId)
        {
            var policy = Policy
                .Handle<ArgumentException>()
                .Retry((e, i) =>
                {
                    Logger.LogError(e, "Key generation failed.");

                    if (i == 5)
                    {
                        throw new InvalidOperationException("An error occurred while generating Ethereum keys.", e);
                    }
                });

            var keys = policy.Execute(GenerateEthereumKeys);

            var address = new CryptoAddress
            {
                UserId = userId,
                Currency = Currency,
                PrivateKey = keys.PrivateKey,
                Type = CryptoAddressType.Investment,
                Address = keys.Address
            };

            var invsetmentAddressEntry = await Context.CryptoAddresses.AddAsync(address);

            // duplicate the same address as the contract address.
            var contractAddress = Mapper.Map<CryptoAddress>(invsetmentAddressEntry.Entity);
            contractAddress.Type = CryptoAddressType.Contract;

            await Context.CryptoAddresses.AddAsync(contractAddress);

            Context.SaveChanges();
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address)
        {
            var uri = new Uri($"{_ethereumSettings.Value.ApiUri}module=account&action=txlist&address={address}&startblock=0&endblock=99999999&sort=asc&apikey={_ethereumSettings.Value.ApiKey}");
            var result = await _restService.GetAsync<EtherscanResponse>(uri);
            return Mapper.Map<List<CryptoTransaction>>(result.Result.Where(x => int.Parse(x.Confirmations) >= _ethereumSettings.Value.Confirmations));
        }

        protected override async Task TransferAssets(CryptoAddress address, string destinationAddress)
        {
            throw new NotImplementedException();
        }

        private (string Address, string PrivateKey) GenerateEthereumKeys()
        {
            var ecKey = EthECKey.GenerateKey();
            var address = ecKey.GetPublicAddress();
            var bytes = ecKey.GetPrivateKeyAsBytes();
            var service = new KeyStorePbkdf2Service();
            var privateKey = service.EncryptAndGenerateKeyStoreAsJson(KeyVaultService.KeyStoreEncryptionPassword, bytes, address);
            return (Address: address, PrivateKey: privateKey);
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
