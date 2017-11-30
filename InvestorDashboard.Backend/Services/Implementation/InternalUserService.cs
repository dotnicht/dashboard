using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
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
        private readonly ICsvService _csvService;
        private readonly IOptions<TokenSettings> _options;
        private readonly IRestService _restService;

        public InternalUserService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            ICsvService csvService,
            IRestService restService,
            IOptions<TokenSettings> options)
            : base(context, loggerFactory)
        {
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
        }

        public async Task SynchronizeInternalUsersData()
        {
            foreach (var record in _csvService.GetRecords<AffiliatesRecord>("InternalUserData.csv"))
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
                            throw new InvalidOperationException($"User not found with email { record.Email }.");
                        }

                        var address = user.CryptoAddresses.SingleOrDefault(x => x.Currency == Currency.DTT)
                            ?? Context.CryptoAddresses.Add(new CryptoAddress { User = user, Currency = Currency.DTT, Type = CryptoAddressType.Internal }).Entity;

                        Context.CryptoTransactions.Add(new CryptoTransaction
                        {
                            Amount = record.DTT,
                            ExternalId = record.Guid,
                            CryptoAddress = address,
                            TokenPrice = _options.Value.Price,
                            Direction = CryptoTransactionDirection.Internal
                        });

                        await Context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while processing record { record.Guid }.");
                    }
                }
            }
        }

        public async Task<decimal> GetInternalUserBalance(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var address = await Context.Users
                .Include(x => x.CryptoAddresses)
                .ThenInclude(x => x.CryptoTransactions)
                .SingleOrDefault(x => x.Id == userId)
                ?.CryptoAddresses
                .ToAsyncEnumerable()
                .SingleOrDefault(x => !x.IsDisabled && x.Type == CryptoAddressType.Internal && x.Currency == Currency.DTT);

            return address
                    ?.CryptoTransactions
                    .Where(x => x.Direction == CryptoTransactionDirection.Internal && x.ExternalId != null)
                    .Sum(x => x.Amount)
                ?? 0;
        }

        private class AffiliatesRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public decimal DTT { get; set; }
        }
    }
}
