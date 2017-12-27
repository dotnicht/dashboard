using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class DashboardHistoryService : ContextService, IDashboardHistoryService
    {
        private readonly IOptions<TokenSettings> _options;
        private readonly IMapper _mapper;

        public DashboardHistoryService(ApplicationDbContext context, ILoggerFactory loggerFactory, IOptions<TokenSettings> options, IMapper mapper)
            : base(context, loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IDictionary<Currency, DashboardHistoryItem>> GetHistoryItems(DateTime? dateTime = null)
        {
            if (!Context.DashboardHistoryItems.Any())
            {
                await RefreshHistory();
            }

            return Context.DashboardHistoryItems
                .Where(x => x.Currency != Currency.USD)
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.Created).Where(y => dateTime == null || y.Created < dateTime.Value).FirstOrDefault());
        }

        public async Task RefreshHistory()
        {
            var items = CreateLatestDashboardHistoryItems();
            await Context.DashboardHistoryItems.AddRangeAsync(items.Values);
            await Context.SaveChangesAsync();
        }

        private IDictionary<Currency, DashboardHistoryItem> CreateLatestDashboardHistoryItems()
        {
            var transactions = Context.CryptoTransactions
                .Include(x => x.CryptoAddress)
                .ThenInclude(x => x.User)
                .Where(x => x.Direction == CryptoTransactionDirection.Inbound && x.CryptoAddress.Type == CryptoAddressType.Investment);

            bool nonInternalPredicate(CryptoTransaction tx) => tx.CryptoAddress.User.ExternalId == null;

            // TODO: optimize the query.
            var result = transactions
                .ToArray()
                .GroupBy(x => x.CryptoAddress.Currency)
                .ToDictionary(
                    x => x.Key,
                    x => new DashboardHistoryItem
                    {
                        IsTokenSaleDisabled = _options.Value.IsTokenSaleDisabled,
                        BonusPercentage = _options.Value.BonusPercentage,
                        TokenPrice = _options.Value.Price,
                        TotalCoins = _options.Value.TotalCoins,
                        Currency = x.Key,
                        TotalUsers = Context.Users.Count(),
                        TotalInvestors = transactions.Select(y => y.CryptoAddress.UserId).Distinct().Count(),
                        TotalInvested = x.Sum(y => y.Amount),
                        TotalUsdInvested = x.Sum(y => y.Amount * y.ExchangeRate),
                        TotalCoinsBought = x.Sum(y => y.Amount * y.ExchangeRate / y.TokenPrice),
                        TotalNonInternalUsers = Context.Users.Count(y => y.ExternalId == null),
                        TotalNonInternalInvestors = transactions.Where(nonInternalPredicate).Select(y => y.CryptoAddress.UserId).Distinct().Count(),
                        TotalNonInternalInvested = x.Where(nonInternalPredicate).Sum(y => y.Amount),
                        TotalNonInternalUsdInvested = x.Where(nonInternalPredicate).Sum(y => y.Amount * y.ExchangeRate),
                        TotalNonInternalCoinsBought = x.Where(nonInternalPredicate).Sum(y => y.Amount * y.ExchangeRate / y.TokenPrice),
                    });

            return result;
        }
    }
}
