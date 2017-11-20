using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
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
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly IOptions<TokenSettings> _options;
        private readonly IMapper _mapper;

        public DashboardHistoryService(ApplicationDbContext context, ILoggerFactory loggerFactory, IEnumerable<ICryptoService> cryptoServices, IOptions<TokenSettings> options, IMapper mapper) 
            : base(context, loggerFactory)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task RefreshHistory()
        {
            var currencies = _cryptoServices.Select(x => x.Settings.Value.Currency).ToArray();

            var transactions = Context.CryptoTransactions.Where(
                x => x.Direction == CryptoTransactionDirection.Inbound
                && x.CryptoAddress.Type == CryptoAddressType.Investment
                && currencies.Contains(x.CryptoAddress.Currency));

            var item = _mapper.Map<DashboardHistoryItem>(_options.Value);

            item.TotalCoinsBought = transactions.Sum(x => x.Amount * x.ExchangeRate / x.TokenPrice);
            item.TotalUsdInvested = transactions.Sum(x => x.Amount * x.ExchangeRate);
            item.TotalInvestors = transactions.Select(x => x.CryptoAddress.UserId)
                .Distinct()
                .Count();

            await Context.DashboardHistoryItems.AddAsync(item);
            await Context.SaveChangesAsync();
        }

        public async Task<DashboardHistoryItem> GetLatestHistoryItem()
        {
            if (!Context.DashboardHistoryItems.Any())
            {
                await RefreshHistory();
            }

            return Context.DashboardHistoryItems.Single(x => x.Created == Context.DashboardHistoryItems.Max(y => y.Created));
        }
    }
}
