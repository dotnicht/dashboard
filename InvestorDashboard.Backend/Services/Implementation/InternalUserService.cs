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
            foreach (var record in _csvService.GetRecords<InternalUserDataRecord>("InternalUserData.csv"))
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
                            TokenPrice = 1,
                            ExchangeRate = 1,
                            Direction = CryptoTransactionDirection.Internal,
                            TimeStamp = DateTime.UtcNow
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

        private class InternalUserDataRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public decimal DTT { get; set; }
        }
    }
}
