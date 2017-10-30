using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using Nethereum.KeyStore;
using Nethereum.Signer;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : CryptoService, IEthereumService
    {
        private readonly IOptions<EthereumSettings> _ethereumSettings;

        public override Currency Currency => Currency.ETH;

        public EthereumService(ApplicationDbContext context, IExchangeRateService exchangeRateService, IKeyVaultService keyVaultService, IMapper mapper, IOptions<TokenSettings> tokenSettings, IOptions<EthereumSettings> ethereumSettings)
            : base(context, exchangeRateService, keyVaultService, mapper, tokenSettings)
            => _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));

        protected override async Task UpdateUserDetailsInternal(string userId)
        {
            var ecKey = EthECKey.GenerateKey();
            var address = ecKey.GetPublicAddress();

            var invsetmentAddress = await Context.CryptoAddresses.AddAsync(new CryptoAddress
            {
                UserId = userId,
                Currency = Currency,
                PrivateKey = new KeyStorePbkdf2Service().EncryptAndGenerateKeyStoreAsJson(KeyVaultService.KeyStoreEncryptionPassword, ecKey.GetPrivateKeyAsBytes(), address),
                Type = CryptoAddressType.Investment,
                Address = address
            });

            // duplicate the same address as the contract address.
            var contractAddress = Mapper.Map<CryptoAddress>(invsetmentAddress.Entity);
            contractAddress.Type = CryptoAddressType.Contract;
            await Context.CryptoAddresses.AddAsync(contractAddress);

            Context.SaveChanges();
        }

        protected override async Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockChain(string address)
        {
            var uri = $"{_ethereumSettings.Value.ApiUri}module=account&action=txlist&address={address}&startblock=0&endblock=99999999&sort=asc&apikey={_ethereumSettings.Value.ApiKey}";
            var result = await RestUtil.Get<EtherscanResponse>(uri);
            return Mapper.Map<List<CryptoTransaction>>(result.Result);
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
