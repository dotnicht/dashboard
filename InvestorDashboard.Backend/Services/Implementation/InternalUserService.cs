using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class InternalUserService : ContextService, IInternalUserService
    {
        private readonly string _kycTransactionHash = Guid.Parse("EBEE4A26-E2B6-42CE-BBF1-D933E70679B4").ToString();
        private readonly IResourceService _resourceService;
        private readonly IOptions<TokenSettings> _options;
        private readonly IRestService _restService;
        private readonly ITokenService _tokenService;

        public InternalUserService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            IResourceService resourceService,
            IRestService restService,
            ITokenService tokenService,
            IOptions<TokenSettings> options)
            : base(context, loggerFactory)
        {
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task SynchronizeInternalUsersData()
        {
            foreach (var record in _resourceService.GetCsvRecords<InternalUserDataRecord>("InternalUserData.csv"))
            {
                if (!Context.CryptoTransactions.Any(x => x.ExternalId == record.Guid))
                {
                    try
                    {
                        var user = Context.Users
                            .Include(x => x.CryptoAddresses)
                            .SingleOrDefault(x => x.Email == record.Email);

                        if (user == null)
                        {
                            throw new InvalidOperationException($"User not found with email {record.Email}.");
                        }

                        CryptoAddress address = EnsureInternalAddress(user);

                        var tx = new CryptoTransaction
                        {
                            Amount = record.Tokens.ToString(),
                            ExternalId = record.Guid,
                            CryptoAddress = address,
                            Direction = CryptoTransactionDirection.Internal,
                            Timestamp = DateTime.UtcNow
                        };

                        await Context.CryptoTransactions.AddAsync(tx);
                        await Context.SaveChangesAsync();
                        await _tokenService.RefreshTokenBalance(user.Id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while processing record {record.Guid}.");
                    }
                }
            }
        }

        public async Task UpdateKycTransaction(string userId = null)
        {
            if (userId == null)
            {
                var ids = Context.Users
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
            else
            {
                await UpdateKycTransactionInternal(userId);
            }
        }

        public bool IsKycDataFilled(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return new [] { user.FirstName, user.LastName, user.CountryCode, user.City, user.PhoneCode, user.PhoneNumber, user.Photo }
                .All(x => !string.IsNullOrWhiteSpace(x));
        }

        private async Task UpdateKycTransactionInternal(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = Context.Users.SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
            }

            var tx = Context.CryptoTransactions.SingleOrDefault(
                x => x.Direction == CryptoTransactionDirection.Internal
                && x.CryptoAddress.Currency == Currency.Token
                && x.CryptoAddress.Type == CryptoAddressType.Internal
                && x.CryptoAddress.UserId == user.Id
                && x.Hash == _kycTransactionHash);

            var filled = IsKycDataFilled(user);

            if (!filled && tx != null)
            {
                Context.CryptoTransactions.Remove(tx);
            }
            else if (filled && tx == null && _options.Value.Bonus.KycBonus != null)
            {
                tx = new CryptoTransaction
                {
                    Amount = (user.KycBonus ?? (user.KycBonus = _options.Value.Bonus.KycBonus)).ToString(),
                    Hash = _kycTransactionHash,
                    CryptoAddress = EnsureInternalAddress(user),
                    Direction = CryptoTransactionDirection.Internal,
                    Timestamp = DateTime.UtcNow
                };

                Context.CryptoTransactions.Add(tx);
            }
            else if (tx != null && user.KycBonus == null)
            {
                user.KycBonus = long.Parse(tx.Amount);
            }

            await Context.SaveChangesAsync();
            await _tokenService.RefreshTokenBalance(user.Id);
        }

        private CryptoAddress EnsureInternalAddress(ApplicationUser user)
        {
            return Context.CryptoAddresses.SingleOrDefault(x => x.Currency == Currency.Token && !x.IsDisabled && x.Type == CryptoAddressType.Internal && x.UserId == user.Id)
                ?? Context.CryptoAddresses.Add(new CryptoAddress { UserId = user.Id, Currency = Currency.Token, Type = CryptoAddressType.Internal }).Entity;
        }

        private class InternalUserDataRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public long Tokens { get; set; }
        }
    }
}
