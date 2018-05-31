using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class CryptoService : ContextService, ICryptoService
    {
        public IOptions<CryptoSettings> Settings { get; }
        public IOptions<ReferralSettings> ReferralSettings { get; }
        protected IOptions<TokenSettings> TokenSettings { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IResourceService ResourceService { get; }
        protected IRestService RestService { get; }
        protected ICalculationService CalculationService { get; }
        protected ITokenService TokenService { get; }
        protected IMapper Mapper { get; }

        protected Func<CryptoAddress, bool> ReferralTransferAddressSelector
        {
            get => x => x.Currency == Settings.Value.Currency && x.Type == CryptoAddressType.Referral && !x.IsDisabled;
        }

        private Expression<Func<CryptoAddress, bool>> InboundAddressSelector
        {
            get => x => x.Currency == Settings.Value.Currency
                && x.Type == CryptoAddressType.Investment
                && x.User.ExternalId == null
                && x.User.EmailConfirmed
                && (!x.IsDisabled || Settings.Value.ImportDisabledAddressesTransactions);
        }

        protected CryptoService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IExchangeRateService exchangeRateService,
            IKeyVaultService keyVaultService,
            IResourceService resourceService,
            IRestService restService,
            ICalculationService calculationService,
            ITokenService tokenService,
            IMapper mapper,
            IOptions<TokenSettings> tokenSettings,
            IOptions<CryptoSettings> cryptoSettings,
            IOptions<ReferralSettings> referralSettings)
            : base(serviceProvider, loggerFactory)
        {
            ExchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            KeyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            TokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            Settings = cryptoSettings ?? throw new ArgumentNullException(nameof(cryptoSettings));
            ReferralSettings = referralSettings ?? throw new ArgumentNullException(nameof(referralSettings));
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

            if (Settings.Value.IsDisabled && Settings.Value.SkipAddressCreationOnDisabled)
            {
                return null;
            }

            using (new ElapsedTimer(Logger, $"CreateCryptoAddress. Currency: {Settings.Value.Currency}. User: {userId}."))
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

            CryptoAddress[] addresses = null;

            using (var ctx = CreateContext())
            {
                addresses = ctx.CryptoAddresses
                    .Where(InboundAddressSelector)
                    .ToArray();
            }

            const string addressKey = "address";

            var policy = Policy
                .Handle<Exception>()
                .Retry(10, (e, i, c) => Logger.LogError(e, $"Transaction list retrieve failed. Currency: {Settings.Value.Currency}. Address: {c[addressKey]}. Retry attempt: {i}."));

            foreach (var address in addresses)
            {
                using (new ElapsedTimer(Logger, $"GetTransactionsFromBlockchain. Currency: {Settings.Value.Currency}. Address: {address.Address}."))
                {
                    foreach (var transaction in await policy.Execute(() => GetTransactionsFromBlockchain(address.Address), new Dictionary<string, object> { { addressKey, address } }))
                    {
                        Logger.LogInformation($"Received {Settings.Value.Currency} transaction list for address {address}.");

                        using (var ctx = CreateContext())
                        {
                            if (!ctx.CryptoTransactions.Any(x => x.Hash == transaction.Hash && x.CryptoAddressId == address.Id && x.Direction == transaction.Direction))
                            {
                                transaction.CryptoAddressId = address.Id;

                                await ctx.CryptoTransactions.AddAsync(transaction);
                                await ctx.SaveChangesAsync();

                                // TODO: send transaction received message.

                                await TokenService.RefreshTokenBalance(address.UserId);
                            }
                        }
                    }
                }

                if (Settings.Value.LegacyTransactionRefreshTimeout != null)
                {
                    await Task.Delay(Settings.Value.LegacyTransactionRefreshTimeout.Value);
                }
            }
        }

        public async Task RefreshTransactionsFromBlockchain()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            using (var ctx = CreateContext())
            {
                var addresses = ctx.CryptoAddresses
                    .Where(InboundAddressSelector)
                    .ToArray();

                await RefreshTransactionsFromBlockchainInternal(addresses, ctx);
            }
        }

        public virtual async Task TransferAvailableAssets()
        {
            if (Settings.Value.IsDisabled)
            {
                return;
            }

            using (var ctx = CreateContext())
            {
                var destination = GetInternalDestinationAddress();

                if (!ReferralSettings.Value.IsDisabled)
                {
                    foreach (var tx in GetReferralTransferAddresses(ctx))
                    {
                        var inbound = tx.First().CryptoAddress;
                        var referral = inbound.User.ReferralUser.CryptoAddresses.Single(ReferralTransferAddressSelector);
                        var balance = await GetBalance(inbound);
                        var value = balance * new BigInteger(ReferralSettings.Value.Reward * 100) / 100;

                        var (hash, adjustedAmount, success) = await PublishTransaction(inbound, referral.Address, value, CryptoTransactionDirection.Referral);
                        if (success)
                        {
                            tx.ToList().ForEach(x => x.IsReferralPaid = true);
                            await ctx.SaveChangesAsync();
                        }

                        (hash, adjustedAmount, success) = await PublishTransaction(inbound, destination, balance - value);
                        if (success)
                        {
                            tx.ToList().ForEach(x => x.IsSpent = true);
                            await ctx.SaveChangesAsync();
                        }
                    }
                }

                foreach (var tx in GetTransferTransactions(ctx))
                {
                    var (hash, adjustedAmount, success) = await PublishTransaction(tx.CryptoAddress, destination);
                    if (success)
                    {
                        tx.IsSpent = true;
                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransaction(CryptoAddress sourceAddress, string destinationAddress, BigInteger? amount = null, CryptoTransactionDirection direction = CryptoTransactionDirection.Internal)
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
                    Direction = direction,
                    CryptoAddressId = sourceAddress.Id
                };

                using (var ctx = CreateContext())
                {
                    await ctx.CryptoTransactions.AddAsync(transaction);
                    await ctx.SaveChangesAsync();
                }
            }

            return result;
        }

        public async Task RefreshTransactionsByBalance()
        {
            CryptoAddress[] addresses = null;

            using (var ctx = CreateContext())
            {
                addresses = await ctx.CryptoAddresses
                    .Where(InboundAddressSelector)
                    .ToArrayAsync();
            }

            var index = 0;
            var result = new ConcurrentQueue<CryptoAddress>();

            using (var elapsed = new ElapsedTimer(Logger, $"RefreshTransactionsByBalance. Currency: {Settings.Value.Currency}. Addresses: {addresses.Count()}."))
            {
                Parallel.ForEach(addresses, new ParallelOptions { MaxDegreeOfParallelism = Settings.Value.MaxDegreeOfParallelism }, async x =>
                {
                    try
                    {
                        var balance = (await GetBalance(x)).ToString();
                        if (x.Balance != balance)
                        {
                            using (var ctx = CreateContext())
                            {
                                ctx.Attach(x);
                                x.Balance = balance;
                                await ctx.SaveChangesAsync();
                                result.Enqueue(x);
                            }
                        }

                        Interlocked.Increment(ref index);

                        if (index % 1000 == 0)
                        {
                            Logger.LogInformation($"Proccessing {index} {Settings.Value.Currency} addresses elapsed {elapsed.Stopwatch.Elapsed}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while checking {Settings.Value.Currency} balance for address {x.Address}.");
                    }
                });
            }

            using (var ctx = CreateContext())
            {
                await RefreshTransactionsFromBlockchainInternal(result.ToArray(), ctx);
            }
        }

        protected string GetInternalDestinationAddress()
        {
            return ResourceService
                    .GetCsvRecords<InternalCryptoAddressDataRecord>("InternalCryptoAddressData.csv")
                    .Where(x => x.Currency == Settings.Value.Currency && x.Environment == Configuration.Environment)
                    .OrderBy(x => Guid.NewGuid())
                    .FirstOrDefault()
                    ?.Address
                ?? throw new InvalidOperationException("Internal addresses not available.");
        }

        protected IGrouping<string, CryptoTransaction>[] GetReferralTransferAddresses(ApplicationDbContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            return ctx.CryptoTransactions
                .Include(x => x.CryptoAddress)
                .ThenInclude(x => x.User)
                .ThenInclude(x => x.ReferralUser)
                .ThenInclude(x => x.CryptoAddresses)
                .Where(
                    x => !x.IsReferralPaid
                    && x.Direction == CryptoTransactionDirection.Inbound
                    && x.CryptoAddress.Currency == Settings.Value.Currency
                    && x.CryptoAddress.Type == CryptoAddressType.Investment
                    && x.CryptoAddress.User.ExternalId == null
                    && x.CryptoAddress.User.ReferralUserId != null)
                .ToArray()
                .Where(x => x.CryptoAddress.User.ReferralUser.CryptoAddresses.Any(ReferralTransferAddressSelector))
                .GroupBy(x => x.CryptoAddress.User.ReferralUser.CryptoAddresses.Single(ReferralTransferAddressSelector).Address)
                .ToArray();
        }

        protected CryptoTransaction[] GetTransferTransactions(ApplicationDbContext ctx)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            return ctx.CryptoTransactions
                .Include(x => x.CryptoAddress)
                .Where(
                    x => !x.IsSpent
                    && x.ExternalId == null
                    && (x.Direction == CryptoTransactionDirection.Inbound || x.Direction == CryptoTransactionDirection.Change)
                    && x.CryptoAddress.Currency == Settings.Value.Currency
                    && !x.CryptoAddress.IsDisabled
                    && x.CryptoAddress.Type == CryptoAddressType.Investment
                    && x.CryptoAddress.User.ExternalId == null)
                .ToArray();
        }

        protected abstract (string Address, string PrivateKey) GenerateKeys(string password = null);
        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
        protected abstract Task<(string Hash, BigInteger AdjustedAmount, bool Success)> PublishTransactionInternal(CryptoAddress sourceAddress, string destinationAddress, BigInteger? amount = null);
        protected abstract Task<long> GetCurrentBlockIndex();
        protected abstract Task ProccessBlock(long index, IEnumerable<CryptoAddress> addresses);
        protected abstract Task<BigInteger> GetBalance(CryptoAddress address);

        private async Task RefreshTransactionsFromBlockchainInternal(CryptoAddress[] cryptoAddresses, ApplicationDbContext ctx)
        {
            if (cryptoAddresses.Any())
            {
                var index = await GetCurrentBlockIndex();

                var addresses = cryptoAddresses
                    .Where(x => (x.LastBlockIndex ?? x.StartBlockIndex) < index)
                    .GroupBy(x => x.LastBlockIndex ?? x.StartBlockIndex)
                    .OrderBy(x => x.Key)
                    .ToArray();

                IEnumerable<CryptoAddress> lastAddresses = null;
                long lastIndex = 0;

                foreach (var item in addresses)
                {
                    lastIndex = item.Key;
                    lastAddresses = (lastAddresses ?? addresses.Where(x => x.Key < item.Key).SelectMany(x => x))
                        .Union(item)
                        .Distinct();

                    await ProccessBlockAndUpdateAddresses(item.Key, lastAddresses, ctx);
                }

                for (var i = lastIndex + 1; i <= index; i++)
                {
                    await ProccessBlockAndUpdateAddresses(i, lastAddresses, ctx);
                }
            }
        }

        private async Task ProccessBlockAndUpdateAddresses(long index, IEnumerable<CryptoAddress> addresses, ApplicationDbContext context)
        {
            using (new ElapsedTimer(Logger, $"ProccessBlockAndUpdateAddresses. Currency: {Settings.Value.Currency}. Block: {index}. Addresses: {addresses.Count()}"))
            {
                await ProccessBlock(index, addresses);
                addresses.ToList().ForEach(x => x.LastBlockIndex = index);
                await context.SaveChangesAsync();
            }
        }

        private async Task<CryptoAddress> CreateAddressInternal(string userId, CryptoAddressType addressType, string address, string privateKey = null)
        {
            using (var ctx = CreateContext())
            {
                var entity = ctx.CryptoAddresses.SingleOrDefault(x => x.Currency == Settings.Value.Currency && x.Type == addressType && x.UserId == userId && !x.IsDisabled);

                if (entity != null)
                {
                    Logger.LogWarning($"Address already exists for user {userId} and currency {Settings.Value.Currency}.");
                    return entity;
                }

                entity = new CryptoAddress
                {
                    UserId = userId,
                    Currency = Settings.Value.Currency,
                    Type = addressType,
                    Address = address,
                    PrivateKey = privateKey
                };

                if (addressType == CryptoAddressType.Investment)
                {
                    entity.StartBlockIndex = await GetCurrentBlockIndex();
                }

                await ctx.CryptoAddresses.AddAsync(entity);
                await ctx.SaveChangesAsync();

                return entity;
            }
        }

        private class InternalCryptoAddressDataRecord
        {
            public Currency Currency { get; set; }
            public string Address { get; set; }
            public Configuration.EnvironmentValue Environment { get; set; }
        }
    }
}
