﻿using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class CryptoService : ContextService, ICryptoService
    {
        public IOptions<CryptoSettings> Settings { get; }

        protected IOptions<TokenSettings> TokenSettings { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IResourceService ResourceService { get; }
        protected IRestService RestService { get; }
        protected ICalculationService CalculationService { get; }
        protected ITokenService TokenService { get; }
        protected IMapper Mapper { get; }

        protected CryptoService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IRestService restService,
            ICalculationService calculationService,
            ITokenService tokenService,
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
            RestService = restService ?? throw new ArgumentNullException(nameof(restService));
            CalculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            TokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<CryptoAddress> CreateCryptoAddress(string userId, string password = null)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (Settings.Value.IsDisabled)
            {
                return null;
            }

            using (new ElapsedTimer(Logger, $"CreateCryptoAddress: {Settings.Value.Currency}"))
            {
                var (address, privateKey) = GenerateKeys(password);
                return await CreateAddressInternal(userId, CryptoAddressType.Investment, address, privateKey);
            }
        }

        public async Task RefreshInboundTransactions()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var addresses = Context.CryptoAddresses
                .Include(x => x.User)
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
                        transaction.CryptoAddressId = address.Id;

                        if (address.User.ReferralUserId != null)
                        {
                            transaction.IsReferralPaid = false;
                        }

                        await Context.CryptoTransactions.AddAsync(transaction);
                        await Context.SaveChangesAsync();

                        // TODO: send transaction received message.

                        await TokenService.RefreshTokenBalance(address.UserId);
                    }
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        public async Task RefreshTransactionsFromBlockchain()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            var index = await GetCurrentBlockIndex();

            var addresses = Context.CryptoAddresses
                .Where(
                    x => x.Currency == Settings.Value.Currency
                    && x.Type == CryptoAddressType.Investment
                    && x.User.ExternalId == null
                    && (!x.IsDisabled || Settings.Value.ImportDisabledAddressesTransactions)
                    && (x.LastBlockIndex ?? x.StartBlockIndex) < index)
                .GroupBy(x => x.LastBlockIndex ?? x.StartBlockIndex)
                .OrderBy(x => x.Key)
                .ToArray();

            if (addresses.Any())
            {
                IEnumerable<CryptoAddress> lastAddresses = null;
                long lastIndex = 0;

                foreach (var item in addresses)
                {
                    lastIndex = item.Key;
                    lastAddresses = (lastAddresses ?? addresses
                        .Where(x => x.Key < item.Key)
                        .SelectMany(x => x))
                        .Union(item)
                        .Distinct();

                    await ProccessBlock(item.Key, lastAddresses);
                }

                for (var i = lastIndex + 1; i <= index; i++)
                {
                    await ProccessBlock(i, lastAddresses);
                }
            }
        }

        public virtual async Task TransferAvailableAssets()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            // TODO: implement referral payments.

            CryptoAddress GetInternalCryptoAddress()
            {
                return Context.CryptoAddresses
                    .Where(
                        x => x.Type == CryptoAddressType.Internal 
                        && !x.IsDisabled 
                        && x.Currency == Settings.Value.Currency 
                        && x.UserId == Settings.Value.InternalTransferUserId)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault();
            }

            var destination = GetInternalCryptoAddress();

            if (destination == null)
            {
                var records = ResourceService
                    .GetCsvRecords<InternalCryptoAddressDataRecord>("InternalCryptoAddressData.csv")
                    .Where(x => x.Currency == Settings.Value.Currency);

                foreach (var record in records)
                {
                    if (!Context.CryptoAddresses.Any(
                        x => x.UserId == Settings.Value.InternalTransferUserId
                        && x.Currency == Settings.Value.Currency
                        && x.Type == CryptoAddressType.Internal
                        && !x.IsDisabled && x.Address == record.Address))
                    {
                        await CreateAddressInternal(Settings.Value.InternalTransferUserId, CryptoAddressType.Internal, record.Address);
                    }
                }
            }

            destination = GetInternalCryptoAddress();

            if (destination == null)
            {
                throw new InvalidOperationException("Internal transfer addresses are not available.");
            }

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

        public async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransaction(CryptoAddress sourceAddress, string destinationAddress, BigInteger? amount = null)
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
                    Amount = result.AdjustedAmount.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Direction = CryptoTransactionDirection.Internal
                };

                await Context.CryptoTransactions.AddAsync(transaction);
                await Context.SaveChangesAsync();
            }

            return result;
        }

        protected abstract (string Address, string PrivateKey) GenerateKeys(string password = null);
        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
        protected abstract Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress sourceAddress, string destinationAddress, BigInteger? amount = null);
        protected abstract Task<long> GetCurrentBlockIndex();
        protected abstract Task ProccessBlock(long index, IEnumerable<CryptoAddress> addresses);

        private async Task<CryptoAddress> CreateAddressInternal(string userId, CryptoAddressType addressType, string address, string privateKey = null)
        {
            if (Context.CryptoAddresses.Any(x => x.Currency == Settings.Value.Currency && x.Type == addressType && x.UserId == userId && !x.IsDisabled))
            {
                throw new InvalidOperationException($"Address already exists for user {userId} and currency {Settings.Value.Currency}.");
            }

            var entity = new CryptoAddress
            {
                UserId = userId,
                Currency = Settings.Value.Currency,
                Type = addressType,
                Address = address,
                PrivateKey = privateKey,
                StartBlockIndex = await GetCurrentBlockIndex()
            };

            await Context.CryptoAddresses.AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        private class InternalCryptoAddressDataRecord
        {
            public Currency Currency { get; set; }
            public string Address { get; set; }
        }
    }
}
