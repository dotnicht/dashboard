using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Contracts;
using Nethereum.Util;
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
        private readonly IOptions<EthereumSettings> _ethereumSettings;

        public SmartContractService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory, 
            IKeyVaultService keyVaultService, 
            IResourceService resourceService, 
            IOptions<EthereumSettings> ethereumSettings) 
            : base(serviceProvider, loggerFactory)
        {
            _keyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _ethereumSettings = ethereumSettings ?? throw new ArgumentNullException(nameof(ethereumSettings));
        }

        public async Task<(string Hash, bool Success)> CallSmartContractTransferFromFunction(CryptoAddress sourceAddress, string destinationAddress, decimal amount)
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

            try
            {
                var transfer = await GetSmartContractFunction("transferFrom");
                var window = Convert.ToInt32(_ethereumSettings.Value.AccountUnlockWindow.TotalSeconds);

                if (await transfer.Web3.Personal.UnlockAccount.SendRequestAsync(transfer.Account.Address, _keyVaultService.MasterKeyStoreEncryptionPassword, window))
                {
                    var hash = await transfer.Function.SendTransactionAsync(transfer.Account.Address, sourceAddress.Address, destinationAddress, UnitConversion.Convert.ToWei(amount));
                    return (Hash: hash, Success: true);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while transfering tokens.");
            }

            return (Hash: null, Success: false);
        }

        public async Task<decimal> CallSmartContractBalanceOfFunction(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var balance = await GetSmartContractFunction("balanceOf");
            return UnitConversion.Convert.FromWei(await balance.Function.CallAsync<BigInteger>(address));
        }

        public async Task RefreshOutboundTransactions()
        {
            using (var ctx = CreateContext())
            {
                var transactions = ctx.CryptoTransactions
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
                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task<(Function Function, Web3 Web3, Account Account)> GetSmartContractFunction(string name)
        {
            var account = Account.LoadFromKeyStore(_resourceService.GetResourceString("MasterKeyStore.json"), _keyVaultService.MasterKeyStoreEncryptionPassword);
            var web3 = new Web3(account, _ethereumSettings.Value.NodeAddress.ToString());

            web3.TransactionManager.DefaultGas = _ethereumSettings.Value.DefaultGas;
            web3.TransactionManager.DefaultGasPrice = await web3.Eth.GasPrice.SendRequestAsync();

            var contract = web3.Eth.GetContract(_resourceService.GetResourceString("ContractAbi.json"), _ethereumSettings.Value.ContractAddress);

            return (Function: contract.GetFunction(name), Web3: web3, Account: account);
        }
    }
}
