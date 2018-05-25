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
        private static readonly Guid _internalTransactionHash = Guid.Parse("55D46758-F52B-4E11-A757-299365320EB6");
        private static readonly Guid _managementTransactionHash = Guid.Parse("00278F09-3292-4BA1-A7B5-1C8F6F1024A5");

        private readonly IResourceService _resourceService;
        private readonly ITokenService _tokenService;
        private readonly IGenericAddressService _genericAddressService;

        private readonly IOptions<TokenSettings> _options;

        public InternalUserService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IResourceService resourceService,
            ITokenService tokenService,
            IGenericAddressService genericAddressService,
            IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
        }

        public async Task<(Guid Id, CryptoTransaction[] Transactions)?> GetManagementTransactions(string email)
        {
            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            using (var ctx = CreateContext())
            {
                var user = await ctx.Users
                    .Include(x => x.CryptoAddresses)
                    .ThenInclude(x => x.CryptoTransactions)
                    .SingleOrDefaultAsync(x => x.Email == email.Trim());

                if (user == null)
                {
                    Logger.LogError($"User not found. Email: {email}.");
                    return null;
                }

                return GetManagementTransactionsInternal(user);
            }
        }

        public async Task<(Guid Id, CryptoTransaction[] Transactions)?> GetManagementTransactions(Guid userId)
        {
            using (var ctx = CreateContext())
            {
                var user = await ctx.Users
                    .Include(x => x.CryptoAddresses)
                    .ThenInclude(x => x.CryptoTransactions)
                    .SingleOrDefaultAsync(x => x.Id == userId.ToString());

                if (user == null)
                {
                    Logger.LogError($"User not found. ID: {userId}.");
                    return null;
                }

                return GetManagementTransactionsInternal(user);
            }
        }

        public async Task AddManagementTransaction(Guid userId, long amount)
        {
            using (var ctx = CreateContext())
            {
                var user = ctx.Users.SingleOrDefault(x => x.Id == userId.ToString());
                if (user == null)
                {
                    throw new InvalidOperationException($"User not found. ID: {userId}.");
                }

                await CreateInternalTransaction(amount, Guid.NewGuid(), _managementTransactionHash, ctx, user);
            }
        }

        public async Task SynchronizeInternalUsersData()
        {
            using (var ctx = CreateContext())
            {
                foreach (var record in _resourceService.GetCsvRecords<InternalUserDataRecord>("InternalUserData.csv"))
                {
                    if (!ctx.CryptoTransactions.Any(x => x.ExternalId == record.Guid))
                    {
                        try
                        {
                            var user = ctx.Users
                                .Include(x => x.CryptoAddresses)
                                .SingleOrDefault(x => x.Email == record.Email);

                            if (user == null)
                            {
                                throw new InvalidOperationException($"User not found. Email: {record.Email}.");
                            }

                            await CreateInternalTransaction(record.Tokens, record.Guid, _internalTransactionHash, ctx, user);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while processing record {record.Guid}.");
                        }
                    }
                }
            }
        }

        private static (Guid Id, CryptoTransaction[] Transactions) GetManagementTransactionsInternal(ApplicationUser user)
        {
            var transactions = user.CryptoAddresses
                .SingleOrDefault(x => x.Currency == Currency.Token && x.Type == CryptoAddressType.Internal && !x.IsDisabled)
                ?.CryptoTransactions
                .Where(x => x.Hash == _managementTransactionHash.ToString())
                .ToArray();

            return (Id: Guid.Parse(user.Id), Transactions: transactions);
        }

        private async Task CreateInternalTransaction(long amount, Guid externalId, Guid hash, ApplicationDbContext ctx, ApplicationUser user)
        {
            var address = await _genericAddressService.EnsureInternalAddress(user);

            var tx = new CryptoTransaction
            {
                Amount = amount.ToString(),
                ExternalId = externalId,
                CryptoAddressId = address.Id,
                Direction = CryptoTransactionDirection.Internal,
                Timestamp = DateTime.UtcNow,
                Hash = hash.ToString()
            };

            await ctx.CryptoTransactions.AddAsync(tx);
            await ctx.SaveChangesAsync();

            await _tokenService.RefreshTokenBalance(user.Id);
        }

        private class InternalUserDataRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public long Tokens { get; set; }
        }
    }
}