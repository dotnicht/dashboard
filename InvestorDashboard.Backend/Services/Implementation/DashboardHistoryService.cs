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

        public async Task RefreshHistory()
        {
            var item = GetLatestDashboardHistoryItem();
            await Context.DashboardHistoryItems.AddAsync(item);
            await Context.SaveChangesAsync();
        }

        public async Task<DashboardHistoryItem> GetLatestHistoryItem(bool includeCurrencies = false)
        {
            if (!Context.DashboardHistoryItems.Any())
            {
                await RefreshHistory();
            }

            if (includeCurrencies)
            {
                return GetLatestDashboardHistoryItem(true);
            }

            return Context.DashboardHistoryItems.OrderByDescending(x => x.Created).First();
        }

        private DashboardHistoryItem GetLatestDashboardHistoryItem(bool includeCurrencies = false)
        {
            var transactions = Context.CryptoTransactions.Where(
                x => x.Direction == CryptoTransactionDirection.Inbound 
                && x.CryptoAddress.Type == CryptoAddressType.Investment);

            var item = _mapper.Map<DashboardHistoryItem>(_options.Value);

            item.Created = DateTime.UtcNow;

            item.TotalUsers = Context.Users.Count();
            item.TotalCoinsBought = transactions.Sum(x => x.Amount * x.ExchangeRate / x.TokenPrice);
            item.TotalUsdInvested = transactions.Sum(x => x.Amount * x.ExchangeRate);
            item.TotalInvestors = transactions
                .Select(x => x.CryptoAddress.UserId)
                .Distinct()
                .Count();

            var nonInternalTransactions = transactions.Where(
                x => x.ExternalId == null
                && x.Hash != null
                && x.CryptoAddress.Currency != Currency.DTT
                && x.CryptoAddress.User.ExternalId == null);

            item.TotalNonInternalUsers = Context.Users.Count(x => x.ExternalId == null);
            item.TotalNonInternalUsdInvested = nonInternalTransactions.Sum(x => x.Amount * x.ExchangeRate);
            item.TotalNonInternalInvestors = nonInternalTransactions
                .Select(x => x.CryptoAddress.UserId)
                .Distinct()
                .Count();

            if (includeCurrencies)
            {
                item.Currencies = Context.CryptoTransactions
                    .Include(x => x.CryptoAddress)
                    .Where(
                        x => x.Direction == CryptoTransactionDirection.Inbound
                        && x.CryptoAddress.Type == CryptoAddressType.Investment
                        && x.ExternalId == null
                        && x.CryptoAddress.Currency != Currency.DTT
                        && x.CryptoAddress.User.ExternalId == null)
                    .ToList()
                    .GroupBy(x => x.CryptoAddress.Currency)
                    .Select(x => (Currency: x.Key, Amount: x.Sum(y => y.Amount)))
                    .ToList();
            }

            return item;
        }
    }
}
