﻿using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;
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
        private readonly IMapper _mapper;
        private readonly IKeyVaultService _keyVaultService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IOptions<EthereumSettings> _ethereumSettings;
        private readonly IOptions<TokenSettings> _tokenSettings;

        public Currency Currency => Currency.ETH;

        public EthereumService(ApplicationDbContext context, IOptions<EthereumSettings> ethereumSettings, IOptions<TokenSettings> tokenSettings,  IMapper mapper, IKeyVaultService keyVaultService, IExchangeRateService exchangeRateService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
        }

        public async Task UpdateUserDetails(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var ecKey = EthECKey.GenerateKey();
            var address = ecKey.GetPublicAddress();

            await _context.CryptoAddresses.AddAsync(new CryptoAddress
            {
                CryptoAccount = new CryptoAccount
                {
                    UserId = userId,
                    Currency = Currency,
                    KeyStore = new KeyStorePbkdf2Service().EncryptAndGenerateKeyStoreAsJson(_keyVaultService.KeyStoreEncryptionPassword, ecKey.GetPrivateKeyAsBytes(), address)
                },
                Type = CryptoAddressType.Investment,
                Address = address
            });

            _context.SaveChanges();
        }

        public async Task RefreshInboundTransactions()
        {
            var hashes = _context.CryptoTransactions.Select(x => x.Hash).ToHashSet();
            foreach (var address in _context.CryptoAddresses.Where(x => x.CryptoAccount.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment).ToArray())
            {
                foreach (var transaction in await GetInboundTransactionsByRecipientAddressFromEtherscan(address.Address))
                {
                    if (!hashes.Contains(transaction.Hash))
                    {
                        var trx = _mapper.Map<CryptoTransaction>(transaction);
                        trx.CryptoAddress = address;
                        trx.Direction = CryptoTransactionDirection.Inbound;
                        trx.ExchangeRate = await _exchangeRateService.GetExchangeRate(Currency.ETH, Currency.USD, trx.TimeStamp, true);
                        trx.TokenPrice = _tokenSettings.Value.Price;
                        _context.CryptoTransactions.Add(trx);
                        _context.SaveChanges();
                    }
                }
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        private async Task<EtherscanResponse.Transaction[]> GetInboundTransactionsByRecipientAddressFromEtherscan(string address)
        {
            var uri = $"{_ethereumSettings.Value.ApiUri}module=account&action=txlist&address={address}&startblock=0&endblock=99999999&sort=asc&apikey={_ethereumSettings.Value.ApiKey}";
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
