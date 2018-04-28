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
using System.Numerics;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class DashboardHistoryService : ContextService, IDashboardHistoryService
    {
        private readonly IOptions<TokenSettings> _options;
        private readonly IMapper _mapper;
        private readonly ICalculationService _calculationService;

        public DashboardHistoryService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IOptions<TokenSettings> options,
            IMapper mapper,
            ICalculationService calculationService)
            : base(serviceProvider, loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        public async Task<IDictionary<Currency, DashboardHistoryItem>> GetHistoryItems(DateTime? dateTime = null)
        {
            // TODO: remove date time based selection.

            using (var ctx = CreateContext())
            {
                var items = ctx.DashboardHistoryItems
                    .ToArray()
                    .GroupBy(x => x.Currency);

                var result = await items.ToAsyncEnumerable().ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.Created).Where(y => dateTime == null || y.Created <= dateTime.Value).FirstOrDefault())
                    ?? await items.ToAsyncEnumerable().ToDictionary(x => x.Key, x => x.OrderBy(y => y.Created).Where(y => dateTime == null || y.Created > dateTime.Value).FirstOrDefault());

                return result;
            }
        }

        public async Task RefreshHistory()
        {
            using (var ctx = CreateContext())
            {
                var items = ctx.CryptoTransactions
                    .Include(x => x.CryptoAddress)
                    .ThenInclude(x => x.User)
                    .Where(x => x.Direction == CryptoTransactionDirection.Inbound && x.CryptoAddress.Type == CryptoAddressType.Investment && x.ExternalId == null)
                    .ToArray()
                    .GroupBy(x => x.CryptoAddress.Currency);

                var result = new Dictionary<Currency, DashboardHistoryItem>();

                foreach (var item in items)
                {
                    var dhi = new DashboardHistoryItem
                    {
                        Currency = item.Key,
                        TotalUsers = ctx.Users.Count(),
                        TotalInvestors = item.Select(x => x.CryptoAddress.UserId).Distinct().Count(),
                        TotalCoinsBoughts = ctx.Users.Sum(x => x.Balance),
                        TotalNonInternalUsers = ctx.Users.Count(x => x.ExternalId == null),
                        TotalNonInternalInvestors = item.Where(x => x.CryptoAddress.User.ExternalId == null).Select(x => x.CryptoAddress.UserId).Distinct().Count(),
                        TotalNonInternalCoinsBoughts = ctx.Users.Where(x => x.ExternalId == null).Sum(x => x.Balance)
                    };

                    var total = BigInteger.Zero;
                    var totalNonInternal = BigInteger.Zero;

                    foreach (var tx in item)
                    {
                        total += BigInteger.Parse(tx.Amount);

                        if (tx.CryptoAddress.User.ExternalId == null)
                        {
                            totalNonInternal += BigInteger.Parse(tx.Amount);
                        }
                    }

                    dhi.TotalInvested = _calculationService.ToDecimalValue(total.ToString(), item.Key);
                    dhi.TotalNonInternalInvested = _calculationService.ToDecimalValue(totalNonInternal.ToString(), item.Key);

                    result.Add(item.Key, dhi);
                }

                foreach (var currency in new[] { Currency.ETH, Currency.BTC })
                {
                    if (!result.ContainsKey(currency))
                    {
                        result.Add(currency, new DashboardHistoryItem
                        {
                            Currency = currency,
                            TotalUsers = ctx.Users.Count(),
                            TotalNonInternalUsers = ctx.Users.Count(y => y.ExternalId == null)
                        });
                    }
                }

                await ctx.DashboardHistoryItems.AddRangeAsync(result.Values);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
