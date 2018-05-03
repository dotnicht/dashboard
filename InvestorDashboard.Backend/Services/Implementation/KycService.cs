using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static InvestorDashboard.Backend.ConfigurationSections.TokenSettings.BonusSettings.KycBonusItem;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class KycService : ContextService, IKycService
    {
        private readonly Guid _kycTransactionHash = Guid.Parse("EBEE4A26-E2B6-42CE-BBF1-D933E70679B4");

        private readonly Dictionary<BonusCriterion, Predicate<ApplicationUser>> _bonusMapping = new Dictionary<BonusCriterion, Predicate<ApplicationUser>>
        {
            { BonusCriterion.Photo, x => !string.IsNullOrWhiteSpace(x.Photo) },
            { BonusCriterion.Telegram, x => !string.IsNullOrWhiteSpace(x.TelegramUsername) },
            { BonusCriterion.Profile, x => new[] { x.FirstName, x.LastName, x.CountryCode, x.City, x.PhoneCode, x.PhoneNumber }.All(y => !string.IsNullOrWhiteSpace(y)) },
            { BonusCriterion.Registration, x => true }
        };

        private readonly IGenericAddressService _genericAddressService;
        private readonly ITokenService _tokenService;
        private readonly IOptions<TokenSettings> _options;

        public KycService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IGenericAddressService genericAddressService, ITokenService tokenService, IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task UpdateKycTransactions(string userId = null)
        {
            if (userId == null)
            {
                using (var ctx = CreateContext())
                {
                    var ids = ctx.Users
                        .Where(x => x.ExternalId == null && x.EmailConfirmed)
                        .Select(x => x.Id)
                        .ToArray();

                    foreach (var id in ids)
                    {
                        try
                        {
                            await UpdateKycTransactionInternal(id);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while updating KYC transaction for user {id}.");
                        }
                    }
                }
            }
            else
            {
                await UpdateKycTransactionInternal(userId);
            }
        }

        public async Task DetectDuplicateKycData(string userId = null)
        {
            if (userId == null)
            {
                using (var ctx = CreateContext())
                {
                    var ids = ctx.Users
                        .Where(x => x.ExternalId == null && x.EmailConfirmed && (!x.HasDuplicateKycData || x.IgnoreDuplicateKycData))
                        .Select(x => x.Id)
                        .ToArray();

                    foreach (var id in ids)
                    {
                        try
                        {
                            await DetectDuplicateKycDataInternal(id);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while detecting duplicate KYC data for user {id}.");
                        }
                    }
                }
            }
            else
            {
                await DetectDuplicateKycDataInternal(userId);
            }
        }

        public async Task<CryptoTransaction[]> GetKycTransactions(string userId, Guid hash)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            using (var ctx = CreateContext())
            {
                return await ctx.CryptoTransactions
                    .Where(
                        x => x.Direction == CryptoTransactionDirection.Internal
                        && x.CryptoAddress.Currency == Currency.Token
                        && x.CryptoAddress.Type == CryptoAddressType.Internal
                        && x.CryptoAddress.UserId == userId
                        && x.Hash == hash.ToString())
                    .ToAsyncEnumerable()
                    .ToArray();
            }
        }

        private async Task UpdateKycTransactionInternal(string userId)
        {
            using (var ctx = CreateContext())
            {
                var user = EnsureUser(userId, ctx);

                var legacyTx = (await GetKycTransactions(userId, _kycTransactionHash)).SingleOrDefault();

                var filled = new[] { user.FirstName, user.LastName, user.CountryCode, user.City, user.PhoneCode, user.PhoneNumber, user.Photo }
                    .All(x => !string.IsNullOrWhiteSpace(x));

                if (legacyTx != null)
                {
                    legacyTx.IsInactive = !filled;
                }
                else if (user.KycBonus != null)
                {
                    var tx = await AddBonusTransaction(ctx, user, user.KycBonus.Value, _kycTransactionHash);
                    tx.IsInactive = !filled;
                }
                else
                {
                    foreach (var item in _bonusMapping)
                    {
                        var bonus = _options.Value.Bonus.KycBonuses.Single(y => y.Criterion == item.Key);

                        var tx = (await GetKycTransactions(userId, bonus.TransationHash)).SingleOrDefault()
                            ?? await AddBonusTransaction(ctx, user, bonus.Value, bonus.TransationHash);

                        tx.IsInactive = !_bonusMapping[item.Key](user);
                    }

                    var referralBonus = _options.Value.Bonus.KycBonuses.Single(x => x.Criterion == BonusCriterion.Referral);

                    var txCount = (await GetKycTransactions(userId, referralBonus.TransationHash)).Count();
                    var referralsCount = ctx.Users.Count(x => x.EmailConfirmed && x.ReferralUserId == user.Id);

                    for (var i = 0; i < referralsCount - txCount; i++)
                    {
                        await AddBonusTransaction(ctx, user, referralBonus.Value, referralBonus.TransationHash);
                    }
                }

                await ctx.SaveChangesAsync();
                await _tokenService.RefreshTokenBalance(user.Id);
            }
        }

        private async Task<CryptoTransaction> AddBonusTransaction(ApplicationDbContext ctx, ApplicationUser user, long amount, Guid hash)
        {
            return ctx.CryptoTransactions.Add(new CryptoTransaction
            {
                CryptoAddress = await _genericAddressService.EnsureInternalAddress(user),
                Amount = amount.ToString(),
                Hash = hash.ToString(),
                Direction = CryptoTransactionDirection.Internal,
                Timestamp = DateTime.UtcNow
            }).Entity;
        }

        private async Task DetectDuplicateKycDataInternal(string userId)
        {
            using (var ctx = CreateContext())
            {
                var user = EnsureUser(userId, ctx);

                if (new[] { user.FirstName, user.LastName, user.CountryCode }.All(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (user.IgnoreDuplicateKycData)
                    {
                        user.HasDuplicateKycData = false;
                    }
                    else if (ctx.Users.Any(x => x.FirstName == user.FirstName && x.LastName == user.LastName && x.CountryCode == user.CountryCode && x.Id != user.Id))
                    {
                        user.HasDuplicateKycData = true;
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        private ApplicationUser EnsureUser(string userId, ApplicationDbContext context)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return context.Users.SingleOrDefault(x => x.Id == userId) ?? throw new InvalidOperationException($"User not found with ID {userId}.");
        }
    }
}
