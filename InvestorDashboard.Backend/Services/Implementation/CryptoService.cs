﻿using AutoMapper;
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
        private readonly IAffiliateService _affiliatesService;

        public IOptions<CryptoSettings> Settings { get; }
        protected IOptions<TokenSettings> TokenSettings { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IEmailService EmailService { get; }
        protected IMapper Mapper { get; }

        protected CryptoService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IEmailService emailService,
            IAffiliateService affiliatesService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<CryptoSettings> cryptoSettings)
            : base(context, loggerFactory)
        {
            ExchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            KeyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            EmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            TokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            Settings = cryptoSettings ?? throw new ArgumentNullException(nameof(cryptoSettings));
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        public Task UpdateUserDetails(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (Settings.Value.IsDisabled)
            {
                return Task.CompletedTask;
            }

            return CreateAddress(userId, CryptoAddressType.Investment);
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
                .Where(x => x.Currency == Settings.Value.Currency && x.Type == CryptoAddressType.Investment && x.User.ExternalId == null && (!x.IsDisabled || Settings.Value.ImportDisabledAdressesTransactions))
                .ToArray();

            const string addressKey = "address";

            var policy = Policy
                .Handle<Exception>()
                .Retry(10, (e, i, c) => Logger.LogError(e, $"Transaction list retrieve failed. Currency: { Settings.Value.Currency }. Address: { c[addressKey] }. Retry attempt: {i}."));

            foreach (var address in addresses)
            {
                var data = new Dictionary<string, object> { { addressKey, address } };
                foreach (var transaction in await policy.Execute(() =>  GetTransactionsFromBlockchain(address.Address), data))
                {
                    Logger.LogInformation($"Received { Settings.Value.Currency } transaction list for address { address }.");

                    if (!hashes.Contains(transaction.Hash))
                    {
                        Logger.LogInformation($"Adding { Settings.Value.Currency } transaction. Hash: { transaction.Hash }.");

                        transaction.CryptoAddress = address;
                        transaction.Direction = CryptoTransactionDirection.Inbound; // TODO: determine transaction type.
                        transaction.ExchangeRate = await ExchangeRateService.GetExchangeRate(Settings.Value.Currency, Currency.USD, transaction.TimeStamp, true);
                        transaction.TokenPrice = TokenSettings.Value.Price;
                        transaction.BonusPercentage = TokenSettings.Value.BonusPercentage;

                        await Context.CryptoTransactions.AddAsync(transaction);
                        await Context.SaveChangesAsync();

                        // TODO: send transaction confirmed email.
                        // TODO: send transaction received bot message.
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }

        public async Task TransferAssets()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var destination = Context.CryptoAddresses
                    .Where(x => x.Type == CryptoAddressType.Internal && !x.IsDisabled)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault()
                ?? await CreateAddress("d8033986-1220-47a8-a43e-98b9842d5c5e", CryptoAddressType.Internal);

            foreach (var address in Context.CryptoAddresses.Where(x => x.Currency == Settings.Value.Currency && x.Type == CryptoAddressType.Investment))
            {
                await TransferAssets(address, destination.Address);
            }
        }

        protected abstract Task<CryptoAddress> CreateAddress(string userId, CryptoAddressType addressType);
        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
        protected abstract Task TransferAssets(CryptoAddress sourceAddress, string destinationAddress);
    }
}
