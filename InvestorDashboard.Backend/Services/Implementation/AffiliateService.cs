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
    internal class AffiliateService : ContextService, IAffiliateService
    {
        private readonly IRestService _restService;
        private readonly ICalculationService _calculationService;
        private readonly IOptions<TokenSettings> _options;

        public AffiliateService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IRestService restService,
            ICalculationService calculationService,
            IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task NotifyTransactionsCreated()
        {
            using (var ctx = CreateContext())
            {
                var transactions = ctx.Users
                    .Where(x => x.ClickId != null)
                    .SelectMany(x => x.CryptoAddresses)
                    .Where(x => x.Type == CryptoAddressType.Investment && x.Address != null && x.PrivateKey != null)
                    .SelectMany(x => x.CryptoTransactions)
                    .Where(x => !x.IsNotified && x.ExternalId == null && x.Hash != null)
                    .ToArray();

                foreach (var tx in transactions)
                {
                    var clickId = ctx.CryptoTransactions
                        .Include(x => x.CryptoAddress)
                        .ThenInclude(x => x.User)
                        .SingleOrDefault(x => x.Id == tx.Id)
                        ?.CryptoAddress
                        ?.User
                        ?.ClickId;

                    if (!string.IsNullOrWhiteSpace(clickId))
                    {
                        var date = tx.Timestamp.ToShortDateString();
                        var time = tx.Timestamp.ToShortTimeString();

                        var amount = _calculationService.ToDecimalValue(tx.Amount, tx.CryptoAddress.Currency);

                        var address = $"http://offers.proffico.affise.com/postback?clickid={clickId}&transactionid={tx.Hash}&date={date}&time={time}&currency={tx.CryptoAddress.Currency}&sum={amount}status=5";
                        var uri = new Uri(address);
                        var response = await _restService.GetAsync<AffiseResponse>(uri);

                        if (response.Status == 1)
                        {
                            tx.IsNotified = true;
                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        public async Task NotifyUserRegistered(string userId = null)
        {
            using (var ctx = CreateContext())
            {
                if (userId == null)
                {
                    var ids = ctx.Users
                        .Where(x => x.ClickId != null && !x.IsNotified)
                        .Select(x => x.Id)
                        .ToArray();

                    foreach (var id in ids)
                    {
                        try
                        {
                            await NotifyUserRegisteredInternal(id);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while notifying affiliate user registration. User: {id}.");
                        }
                    }
                }
                else
                {
                    await NotifyUserRegisteredInternal(userId);
                }
            }
        }

        private async Task NotifyUserRegisteredInternal(string userId)
        {
            using (var ctx = CreateContext())
            {
                var user = ctx.Users.Single(x => x.Id == userId);

                if (!string.IsNullOrWhiteSpace(user.ClickId))
                {
                    var address = $"http://offers.proffico.affise.com/postback?clickid={user.ClickId}&goal=2";
                    var uri = new Uri(address);
                    var response = await _restService.GetAsync<AffiseResponse>(uri);

                    if (response.Status == 1)
                    {
                        user.IsNotified = true;
                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }

        private class AffiseResponse
        {
            public int Status { get; set; }
            public string Message { get; set; }
        }
    }
}
