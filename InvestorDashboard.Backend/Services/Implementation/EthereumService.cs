using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Signer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class EthereumService : IEthereumService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<EthereumSettings> _options;
        private readonly IMapper _mapper;
        private readonly IKeyVaultService _keyVaultService;
        private readonly IExchangeRateService _exchangeRateService;

        public Currency Currency => Currency.ETH;

        public EthereumService(ApplicationDbContext context, IOptions<EthereumSettings> options, IMapper mapper, IKeyVaultService keyVaultService, IExchangeRateService exchangeRateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        public EthereumAccount CreateAccount()
        {
            var ecKey = EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKeyAsBytes().ToHex();
            var keyStoreService = new KeyStorePbkdf2Service();
            var bytes = ecKey.GetPrivateKeyAsBytes();
            var address = ecKey.GetPublicAddress();
            var json = keyStoreService.EncryptAndGenerateKeyStoreAsJson(_keyVaultService.KeyStoreEncryptionPassword, bytes, address);

            return new EthereumAccount
            {
                Address = ecKey.GetPublicAddress(),
                KeyStore = json
            };
        }

        public async Task RefreshInboundTransactions()
        {
            var tokenRate = await _exchangeRateService.GetExchangeRate(Currency.DTT, Currency.USD);
            var ethRate = await _exchangeRateService.GetExchangeRate(Currency.ETH, Currency.USD, DateTime.UtcNow, true);
            var hashes = _context.CryptoTransactions.Select(x => x.Hash).ToHashSet();

            foreach (var address in _context.CryptoAddresses.Where(x => x.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment))
            {
                foreach (var transaction in await GetInboundTransactionsByRecipientAddressFromEtherscan(address.Address))
                {
                    if (!hashes.Contains(transaction.Hash))
                    {
                        var trx = _mapper.Map<Database.Models.CryptoTransaction>(transaction);
                        trx.Address = address;
                        trx.Direction = CryptoTransactionDirection.Inbound;
                        trx.ExchangeRate = ethRate;
                        trx.TokenPrice = tokenRate;
                        await _context.CryptoTransactions.AddAsync(trx);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task<EtherscanResponse.Transaction[]> GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_options.Value.ApiUri}module=account&action=txlist&address={address}&startblock=0&endblock=99999999&sort=asc&apikey={_options.Value.ApiKey}";
            var result = await RestUtil.Get<EtherscanResponse>(uri);
            return result?.Result?.ToArray() ?? throw new InvalidOperationException("An error occurred while retrieving transaction list from etherscan.io.");
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
