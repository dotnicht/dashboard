using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class AffiliateService : ContextService, IAffiliateService
    {
        private readonly ICsvService _csvService;
        private readonly IOptions<TokenSettings> _options;
        private readonly IRestService _restService;

        public AffiliateService(
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

        public async Task SyncAffiliates()
        {
            foreach (var record in _csvService.GetRecords<AffiliatesRecord>("AffiliatesData.csv"))
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

        public async Task<decimal> GetUserAffilicateBalance(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return Context.Users
                    .Include(x => x.CryptoAddresses)
                    .ThenInclude(x => x.CryptoTransactions)
                    .SingleOrDefault(x => x.Id == userId)
                    ?.CryptoAddresses
                    .SingleOrDefault(x => !x.IsDisabled && x.Type == CryptoAddressType.Internal && x.Currency == Currency.DTT)
                    ?.CryptoTransactions
                    .Where(x => x.Direction == CryptoTransactionDirection.Internal && x.ExternalId != null)
                    .Sum(x => x.Amount)
                ?? 0;
        }

        public async Task NotifyTransactionsCreated()
        {
            var transactions = Context.Users
                .Where(x => x.ClickId != null)
                .SelectMany(x => x.CryptoAddresses)
                .Where(x => x.Type == CryptoAddressType.Investment && x.Address != null && x.PrivateKey != null)
                .SelectMany(x => x.CryptoTransactions)
                .Where(x => !x.IsNotified && x.ExternalId == null && x.Hash != null)
                .ToArray();

            foreach (var tx in transactions)
            {
                var clickId = Context.CryptoTransactions
                    .Include(x => x.CryptoAddress)
                    .ThenInclude(x => x.User)
                    .SingleOrDefault(x => x.Id == tx.Id)
                    ?.CryptoAddress
                    ?.User
                    ?.ClickId;

                if (!string.IsNullOrWhiteSpace(clickId))
                {
                    var amount = tx.Amount * tx.ExchangeRate;
                    var date = tx.TimeStamp.ToShortDateString();
                    var time = tx.TimeStamp.ToShortTimeString();

                    var address = $"http://offers.proffico.affise.com/postback?clickid={ clickId }&action_id={ tx.Hash }&sum={ amount }&currency=USD&custom_field1={ date }&custom_field2={ time }&custom_field3={ tx.CryptoAddress.Currency }&custom_field4={ tx.Amount }&custom_field5={ tx.ExchangeRate }";

                    var uri = new Uri(address);
                    var response = await _restService.GetAsync<AffiseResponse>(uri);

                    if (response.Status == 1)
                    {
                        tx.IsNotified = true;
                        await Context.SaveChangesAsync();
                    }
                }
            }
        }

        private class AffiliatesRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public decimal DTT { get; set; }
        }

        private class AffiseResponse
        {
            public int Status { get; set; }
            public string Message { get; set; }
        }
    }
}
