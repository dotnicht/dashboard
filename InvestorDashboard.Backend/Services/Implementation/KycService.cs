using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static InvestorDashboard.Backend.ConfigurationSections.TokenSettings.BonusSettings;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class KycService : ContextService, IKycService
    {
        private static readonly Guid _kycTransactionHash = Guid.Parse("EBEE4A26-E2B6-42CE-BBF1-D933E70679B4");
        private static readonly Dictionary<BonusCriterion, Func<ApplicationUser, string>[]> _bonusMapping
            = new Dictionary<BonusCriterion, Func<ApplicationUser, string>[]>
        {
                { BonusCriterion.Photo, new Func<ApplicationUser, string>[] { x => x.Photo } },
                { BonusCriterion.Telegram, new Func<ApplicationUser, string>[] { x => x.TelegramUsername } },
                { BonusCriterion.Profile, new Func<ApplicationUser, string>[] { x => x.FirstName, x => x.LastName, x => x.CountryCode, x => x.City, x => x.PhoneCode, x => x.PhoneNumber } }
        };

        private readonly IGenericAddressService _genericAddressService;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IOptions<TokenSettings> _options;

        public KycService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IGenericAddressService genericAddressService,
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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

        public async Task<CryptoTransaction[]> GetKycTransactions(string userId, params Guid[] hash)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (hash == null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            using (var ctx = CreateContext())
            {
                return await GetKycTransactionsInternal(userId, ctx, hash);
            }
        }

        public async Task<Dictionary<BonusCriterion, (bool Status, long Amount)>> UpdateUserKycData(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var ctx = CreateContext())
            {
                var existing = ctx.Users.Single(x => x.Id == user.Id);

                if (_bonusMapping.SelectMany(x => x.Value).Any(x => !string.IsNullOrWhiteSpace(x(existing))))
                {
                    var profile = _mapper.Map<UserProfile>(existing);
                    profile.UserId = existing.Id;
                    ctx.UserProfiles.Add(profile);
                    ctx.SaveChanges();
                }

                var status = await _userManager.UpdateAsync(user);

                if (!status.Succeeded)
                {
                    throw new InvalidOperationException($"An error occurred while updating user KYC data. {string.Join(". ", status.Errors.Select(x => x.Description))}");
                }

                return GetUserKycDataStatusInternal(user);
            }
        }

        public async Task<Dictionary<BonusCriterion, (bool Status, long Amount)>> GetUserKycDataStatus(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = await _userManager.FindByIdAsync(userId);
            return GetUserKycDataStatusInternal(user);
        }

        private async Task UpdateKycTransactionInternal(string userId)
        {
            using (var ctx = CreateContext())
            {
                var user = EnsureUser(userId, ctx);

                if (!user.UseNewBonusSystem)
                {
                    if (user.KycBonus != null)
                    {
                        await EnsureInternalTransaction(ctx, user, new KycBonusItem { Hash = _kycTransactionHash, Amount = user.KycBonus.Value }, !IsUserLegacyProfileFilled(user));
                    }

                    var hash = _options.Value.Bonus.KycBonuses.Select(x => x.Value.Hash).ToArray();
                    foreach (var tx in await GetKycTransactionsInternal(userId, ctx, hash))
                    {
                        tx.IsInactive = true;
                    }
                }
                else
                {
                    foreach (var tx in await GetKycTransactionsInternal(userId, ctx, _kycTransactionHash))
                    {
                        tx.IsInactive = true;
                    }

                    foreach (var item in _bonusMapping)
                    {
                        var status = item.Value.Any(x => string.IsNullOrWhiteSpace(x(user)));
                        await EnsureInternalTransaction(ctx, user, _options.Value.Bonus.KycBonuses[item.Key], status);
                    }

                    var registration = await GetKycTransactionsInternal(userId, ctx, _options.Value.Bonus.KycBonuses[BonusCriterion.Registration].Hash);

                    if (registration.SingleOrDefault() == null)
                    {
                        await AddBonusTransaction(ctx, user, _options.Value.Bonus.KycBonuses[BonusCriterion.Registration].Amount, _options.Value.Bonus.KycBonuses[BonusCriterion.Registration].Hash);
                    }

                    var referrals = await GetKycTransactionsInternal(userId, ctx, _options.Value.Bonus.KycBonuses[BonusCriterion.Referral].Hash);
                    var referralsCount = ctx.Users.Count(x => x.EmailConfirmed && x.ReferralUserId == user.Id);

                    for (var i = 0; i < referralsCount - referrals.Count(); i++)
                    {
                        await AddBonusTransaction(ctx, user, _options.Value.Bonus.KycBonuses[BonusCriterion.Referral].Amount, _options.Value.Bonus.KycBonuses[BonusCriterion.Referral].Hash);
                    }
                }

                await ctx.SaveChangesAsync();
                await _tokenService.RefreshTokenBalance(user.Id);
            }
        }

        private async Task EnsureInternalTransaction(ApplicationDbContext ctx, ApplicationUser user, KycBonusItem kycBonusItem, bool isInactive)
        {
            var tx = (await GetKycTransactionsInternal(user.Id, ctx, kycBonusItem.Hash)).SingleOrDefault()
                ?? await AddBonusTransaction(ctx, user, kycBonusItem.Amount, kycBonusItem.Hash);

            if (ctx.Entry(tx).State != EntityState.Added)
            {
                ctx.Attach(tx);
            }

            tx.IsInactive = isInactive;
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

        private async Task<CryptoTransaction> AddBonusTransaction(ApplicationDbContext ctx, ApplicationUser user, long amount, Guid hash)
        {
            var address = await _genericAddressService.EnsureInternalAddress(user);

            var tx = new CryptoTransaction
            {
                CryptoAddressId = address.Id,
                Amount = amount.ToString(),
                Hash = hash.ToString(),
                Direction = CryptoTransactionDirection.Internal,
                Timestamp = DateTime.UtcNow,
                ExternalId = Guid.NewGuid()
            };

            return ctx.CryptoTransactions.Add(tx).Entity;
        }

        private Dictionary<BonusCriterion, (bool Status, long Amount)> GetUserKycDataStatusInternal(ApplicationUser user)
        {
            return user.UseNewBonusSystem
                ? _bonusMapping.ToDictionary(x => x.Key, x => (Status: x.Value.All(y => !string.IsNullOrWhiteSpace(y(user))), Amount: _options.Value.Bonus.KycBonuses[x.Key].Amount))
                : new Dictionary<BonusCriterion, (bool Status, long Amount)>
                {
                    { BonusCriterion.Legacy, (Status: IsUserLegacyProfileFilled(user), Amount: user.KycBonus ?? throw new InvalidOperationException($"Invalid KYC data detacted for user {user.Id}.")) }
                };
        }

        private static bool IsUserLegacyProfileFilled(ApplicationUser user)
        {
            return _bonusMapping
                .Where(x => x.Key != BonusCriterion.Telegram)
                .SelectMany(x => x.Value)
                .All(x => !string.IsNullOrWhiteSpace(x(user)));
        }

        private static ApplicationUser EnsureUser(string userId, ApplicationDbContext context)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return context.Users.SingleOrDefault(x => x.Id == userId) ?? throw new InvalidOperationException($"User not found with ID {userId}.");
        }

        private static async Task<CryptoTransaction[]> GetKycTransactionsInternal(string userId, ApplicationDbContext ctx, params Guid[] hash)
        {
            var hashes = hash.Select(y => y.ToString());
            return await ctx.CryptoTransactions
                .Where(
                    x => x.Direction == CryptoTransactionDirection.Internal
                    && x.CryptoAddress.Currency == Currency.Token
                    && x.CryptoAddress.Type == CryptoAddressType.Internal
                    && x.CryptoAddress.UserId == userId
                    && hashes.Contains(x.Hash))
                .ToAsyncEnumerable()
                .ToArray();
        }
    }
}
