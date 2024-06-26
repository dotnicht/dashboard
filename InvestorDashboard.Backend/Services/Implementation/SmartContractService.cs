﻿using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class SmartContractService : ContextService, ISmartContractService
    {
        private readonly IKeyVaultService _keyVaultService;
        private readonly IResourceService _resourceService;
        private readonly ILogger<SmartContractService> _logger;
        private readonly IOptions<EthereumSettings> _ethereumSettings;

        private Web3 _web3;
        private Web3 Web3 { get => _web3 ?? (_web3 = new Web3(Account, _ethereumSettings.Value.NodeAddress.ToString())); }

        private Account _account;
        private Account Account { get => _account ?? (_account = Account.LoadFromKeyStore(_resourceService.GetResourceString("MasterKeyStore.json"), _keyVaultService.MasterKeyStoreEncryptionPassword)); }

        public SmartContractService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            ILogger<SmartContractService> logger,
            IOptions<EthereumSettings> ethereumSettings)
            : base(serviceProvider, loggerFactory)
        {
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
        }

        public async Task<(string Hash, bool Success)> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, BigInteger amount)
        {
            if (sourceAddress == null)
            {
                throw new ArgumentNullException(nameof(sourceAddress));
            }

            if (destinationAddress == null)
            {
                throw new ArgumentNullException(nameof(destinationAddress));
            }

            destinationAddress = destinationAddress.Trim();

            if (destinationAddress.Equals(sourceAddress.Address, StringComparison.InvariantCultureIgnoreCase)
                || destinationAddress.Equals(_ethereumSettings.Value.ContractAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException("Destination address is invalid for token transfer.");
            }

            return await SendSmartContractTransaction("transferFrom", sourceAddress.Address, destinationAddress, amount * new BigInteger(Math.Pow(10, _ethereumSettings.Value.Denomination)));
        }

        public async Task<(string Hash, bool Success)> CallSmartContractMintTokensFunction(string destinationAddress, BigInteger amount)
        {
            if (destinationAddress == null)
            {
                throw new ArgumentNullException(nameof(destinationAddress));
            }

            return await SendSmartContractTransaction("mintTokens", destinationAddress, amount * new BigInteger(Math.Pow(10, _ethereumSettings.Value.Denomination)));
        }

        public async Task<BigInteger> CallSmartContractBalanceOfFunction(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var balance = await GetSmartContractFunction("balanceOf");
            return await balance.CallAsync<BigInteger>(address);
        }

        public async Task<bool?> GetTransactionReceipt(string hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var receipt = await Web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash);
            return receipt == null ? default(bool?) : receipt.Status.Value == 0;
        }

        public async Task<bool> TransactionExists(string hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var tx = await Web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);
            return tx != null;
        }

        private async Task<(string Hash, bool Success)> SendSmartContractTransaction(string functionName, params object[] functionInput)
        {
            if (functionInput == null)
            {
                throw new ArgumentNullException(nameof(functionInput));
            }

            try
            {
                var transfer = await GetSmartContractFunction(functionName);
                var hash = await transfer.SendTransactionAsync(Account.Address, functionInput);
                return (Hash: hash, Success: true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An error occurred while calling smart contract function {functionName}.");
            }

            return (Hash: null, Success: false);
        }

        private async Task<Function> GetSmartContractFunction(string name)
        {
            Web3.TransactionManager.DefaultGas = _ethereumSettings.Value.DefaultGas;
            Web3.TransactionManager.DefaultGasPrice = await Web3.Eth.GasPrice.SendRequestAsync();
            var contract = Web3.Eth.GetContract(_resourceService.GetResourceString("ContractAbi.json"), _ethereumSettings.Value.ContractAddress);
            return contract.GetFunction(name);
        }
    }
}
