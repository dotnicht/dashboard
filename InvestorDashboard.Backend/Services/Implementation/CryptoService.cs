using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class CryptoService : ContextService, ICryptoService
    {
        private readonly IMessageService _messageService;
        private readonly IDashboardHistoryService _dashboardHistoryService;

        public IOptions<CryptoSettings> Settings { get; }

        protected IOptions<TokenSettings> TokenSettings { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IResourceService ResourceService { get; }
        protected IMapper Mapper { get; }

        protected CryptoService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IMessageService messageService,
            IDashboardHistoryService dashboardHistoryService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<CryptoSettings> cryptoSettings)
            : base(context, loggerFactory)
        {
            ExchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            KeyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            TokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            Settings = cryptoSettings ?? throw new ArgumentNullException(nameof(cryptoSettings));
            ResourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _dashboardHistoryService = dashboardHistoryService;
        }

        public async Task<CryptoAddress> CreateCryptoAddress(string userId, CryptoAddressType cryptoAddressType = CryptoAddressType.Investment, string password = null)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (cryptoAddressType != CryptoAddressType.Investment && !(cryptoAddressType == CryptoAddressType.Master && Settings.Value.Currency == Currency.ETH))
            {
                throw new NotSupportedException();
            }

            return Settings.Value.IsDisabled
                ? null
                : await CreateAddress(userId, cryptoAddressType, password);
        }

        public async Task RefreshInboundTransactions()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var addresses = Context.CryptoAddresses
                .Where(
                    x => x.Currency == Settings.Value.Currency
                    && x.Type == CryptoAddressType.Investment
                    && x.User.ExternalId == null
                    && (!x.IsDisabled || Settings.Value.ImportDisabledAddressesTransactions))
                .ToArray();

            const string addressKey = "address";

            var policy = Policy
                .Handle<Exception>()
                .Retry(10, (e, i, c) => Logger.LogError(e, $"Transaction list retrieve failed. Currency: {Settings.Value.Currency}. Address: {c[addressKey]}. Retry attempt: {i}."));

            foreach (var address in addresses)
            {
                foreach (var transaction in await policy.Execute(() => GetTransactionsFromBlockchain(address.Address), new Dictionary<string, object> { { addressKey, address } }))
                {
                    Logger.LogInformation($"Received {Settings.Value.Currency} transaction list for address {address}.");

                    if (!Context.CryptoTransactions.Any(x => x.Hash == transaction.Hash))
                    {
                        await FillAndSaveTransaction(transaction, address);
                        // TODO: send transaction received message.
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        public virtual async Task TransferAvailableAssets()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var destination = Context.CryptoAddresses
                    .Where(x => x.Type == CryptoAddressType.Internal && !x.IsDisabled && x.Currency == Settings.Value.Currency)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault()
                ?? await CreateAddress(Settings.Value.InternalTransferUserId, CryptoAddressType.Internal);

            var sourceAddresses = Context.CryptoAddresses
                .Where(
                    x => x.Currency == Settings.Value.Currency
                    && x.Type == CryptoAddressType.Investment
                    && x.CryptoTransactions.Any()
                    && x.User.ExternalId == null)
                .ToArray();

            foreach (var address in sourceAddresses)
            {
                await PublishTransaction(address, destination.Address);
            }
        }

        public async Task<(string Hash, decimal AdjustedAmount, bool Success)> PublishTransaction(CryptoAddress sourceAddress, string destinationAddress, decimal? amount = null)
        {
            if (sourceAddress == null)
            {
                throw new ArgumentNullException(nameof(sourceAddress));
            }

            if (destinationAddress == null)
            {
                throw new ArgumentNullException(nameof(destinationAddress));
            }

            var result = await PublishTransactionInternal(sourceAddress, destinationAddress, amount);

            if (result.Success)
            {
                var transaction = new CryptoTransaction
                {
                    Hash = result.Hash,
                    Amount = result.AdjustedAmount,
                    TimeStamp = DateTime.UtcNow
                };

                await FillAndSaveTransaction(transaction, sourceAddress, CryptoTransactionDirection.Internal);
            }

            return result;
        }

        protected abstract (string Address, string PrivateKey) GenerateKeys(string password = null);
        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
        protected abstract Task<(string Hash, decimal AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress sourceAddress, string destinationAddress, decimal? amount = null);

        private async Task FillAndSaveTransaction(CryptoTransaction transaction, CryptoAddress address, CryptoTransactionDirection? direction = null)
        {
            Logger.LogInformation($"Adding {Settings.Value.Currency} transaction. Hash: {transaction.Hash}.");

            if (direction != null)
            {
                transaction.Direction = direction.Value;
            }

            var item = (await _dashboardHistoryService.GetHistoryItems(transaction.TimeStamp)).FirstOrDefault().Value;

            transaction.CryptoAddressId = address.Id;
            transaction.ExchangeRate = await ExchangeRateService.GetExchangeRate(Settings.Value.Currency, Currency.USD, transaction.TimeStamp, true);
            transaction.TokenPrice = item?.TokenPrice ?? TokenSettings.Value.Price;
            transaction.BonusPercentage = item?.BonusPercentage ?? TokenSettings.Value.BonusPercentage;

            await Context.CryptoTransactions.AddAsync(transaction);
            await Context.SaveChangesAsync();
        }

        private async Task<CryptoAddress> CreateAddress(string userId, CryptoAddressType addressType, string password = null)
        {
            if (addressType == CryptoAddressType.Internal)
            {
                return await ResourceService.GetCsvRecords<InternalCryptoAddressDataRecord>("InternalCryptoAddressData.csv")
                    .Where(x => x.Currency == Settings.Value.Currency)
                    .Select(async x => await CreateAddressInternal(userId, addressType, x.Address))
                    .ToArray()
                    .FirstOrDefault();
            }

            var (address, privateKey) = GenerateKeys(password);
            return await CreateAddressInternal(userId, addressType, address, privateKey);
        }

        private async Task<CryptoAddress> CreateAddressInternal(string userId, CryptoAddressType addressType, string address, string privateKey = null)
        {
            var result = await Context.CryptoAddresses.AddAsync(new CryptoAddress
            {
                UserId = userId,
                Currency = Settings.Value.Currency,
                Type = addressType,
                Address = address,
                PrivateKey = privateKey
            });

            // TODO: investigate async behaviour here.
            Context.SaveChanges();
            return result.Entity;
        }

        private class InternalCryptoAddressDataRecord
        {
            public Currency Currency { get; set; }
            public string Address { get; set; }
        }
    }
}
