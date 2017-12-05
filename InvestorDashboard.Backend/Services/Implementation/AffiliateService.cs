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

        public AffiliateService(ApplicationDbContext context, ILoggerFactory loggerFactory, IRestService restService)
            : base(context, loggerFactory)
        {
            _restService = restService ?? throw new ArgumentNullException(nameof(restService));
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

                    var address = $"http://offers.proffico.affise.com/postback?clickid={ clickId }&action_id={ tx.Hash }&sum={ amount }&currency=USD&custom_field1={ date }&custom_field2={ time }&custom_field3={ tx.CryptoAddress.Currency }&custom_field4={ tx.Amount }&custom_field5={ tx.ExchangeRate }&status=5";

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

        private class AffiseResponse
        {
            public int Status { get; set; }
            public string Message { get; set; }
        }
    }
}
