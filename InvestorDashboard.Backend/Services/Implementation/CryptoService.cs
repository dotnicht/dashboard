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
        private readonly ICsvService _csvService;
        private readonly IMessageService _messageService;
        private readonly IAffiliateService _affiliatesService;

        public IOptions<CryptoSettings> Settings { get; }
        protected IOptions<TokenSettings> TokenSettings { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IMapper Mapper { get; }

        protected CryptoService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            ICsvService csvService,
            IMessageService messageService,
            IAffiliateService affiliatesService,
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
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
            _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        public Task<CryptoAddress> CreateCryptoAddress(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return Settings.Value.IsDisabled 
                ? null 
                : CreateAddress(userId, CryptoAddressType.Investment);
        }

        public async Task RefreshInboundTransactions()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var hashes = Context.CryptoTransactions
                .Where(x => x.Hash != null)
                .Select(x => x.Hash)
                .ToHashSet();

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
                .Retry(10, (e, i, c) => Logger.LogError(e, $"Transaction list retrieve failed. Currency: { Settings.Value.Currency }. Address: { c[addressKey] }. Retry attempt: {i}."));

            foreach (var address in addresses)
            {
                var data = new Dictionary<string, object> { { addressKey, address } };
                foreach (var transaction in await policy.Execute(() => GetTransactionsFromBlockchain(address.Address), data))
                {
                    Logger.LogInformation($"Received { Settings.Value.Currency } transaction list for address { address }.");

                    if (!hashes.Contains(transaction.Hash))
                    {
                        await FillAndSaveTransaction(transaction, address, CryptoTransactionDirection.Inbound);
                        // TODO: send transaction received message.
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        public async Task TransferAvailableAssets()
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
                .Where(x => x.Currency == Settings.Value.Currency
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

        protected abstract (string Address, string PrivateKey) GenerateKeys();
        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
        protected abstract Task<(string Hash, decimal AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress sourceAddress, string destinationAddress, decimal? amount = null);

        private async Task FillAndSaveTransaction(CryptoTransaction transaction, CryptoAddress address, CryptoTransactionDirection direction)
        {
            Logger.LogInformation($"Adding { Settings.Value.Currency } transaction. Hash: { transaction.Hash }.");

            transaction.Direction = direction;
            transaction.CryptoAddressId = address.Id;
            transaction.ExchangeRate = await ExchangeRateService.GetExchangeRate(Settings.Value.Currency, Currency.USD, transaction.TimeStamp, true);
            transaction.TokenPrice = TokenSettings.Value.Price;
            transaction.BonusPercentage = TokenSettings.Value.BonusPercentage;

            await Context.CryptoTransactions.AddAsync(transaction);
            await Context.SaveChangesAsync();
        }

        private async Task<CryptoAddress> CreateAddress(string userId, CryptoAddressType addressType)
        {
            if (addressType == CryptoAddressType.Internal)
            {
                return await _csvService.GetRecords<InternalCryptoAddressDataRecord>("InternalCryptoAddressData.csv")
                    .Where(x => x.Currency == Settings.Value.Currency)
                    .Select(async x => await CreateAddressInternal(userId, addressType, x.Address))
                    .FirstOrDefault();
            }

            var (Address, PrivateKey) = GenerateKeys();
            return await CreateAddressInternal(userId, addressType, Address, PrivateKey);
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
