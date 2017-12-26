using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ExternalInvestorService : ContextService, IExternalInvestorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICsvService _csvService;
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public ExternalInvestorService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IExchangeRateService exchangeRateService,
            ICsvService csvService,
            IDashboardHistoryService dashboardHistoryService,
            IEnumerable<ICryptoService> cryptoServices,
            IOptions<TokenSettings> tokenSettings)
            : base(context, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        public async Task SynchronizeInvestorsData()
        {
            var records = _csvService.GetRecords<ExternalInvestorDataRecord>("ExternalInvestorData.csv")
                .Where(x => x.DateTime < DateTime.UtcNow)
                .ToArray();

            foreach (var record in records)
            {
                if (!Context.Users.Any(x => x.ExternalId == record.Id))
                {
                    var user = new ApplicationUser
                    {
                        Email = $"{ record.Id }@data-trading.com",
                        UserName = record.Id.ToString(),
                        ExternalId = record.Id
                    };

                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        try
                        {
                            (Currency Currency, decimal Amount) value = record.ETH > 0
                                ? (Currency.ETH, record.ETH)
                                : record.BTC > 0
                                    ? (Currency.BTC, record.BTC)
                                    : throw new InvalidOperationException($"Failed to create external investor. ID { record.Id }.");

                            var address = await _cryptoServices
                                .Single(x => x.Settings.Value.Currency == value.Currency)
                                .CreateCryptoAddress(user.Id);

                            var item = await _dashboardHistoryService.GetClosestHistoryItem(record.DateTime);

                            var transaction = new CryptoTransaction
                            {
                                Amount = value.Amount,
                                Direction = CryptoTransactionDirection.Inbound,
                                ExchangeRate = await _exchangeRateService.GetExchangeRate(value.Currency, Currency.USD, record.DateTime, true),
                                TokenPrice = item?.TokenPrice ?? _tokenSettings.Value.Price,
                                BonusPercentage = item?.BonusPercentage ?? _tokenSettings.Value.BonusPercentage,
                                CryptoAddressId = address.Id,
                                TimeStamp = record.DateTime
                            };

                            await Context.CryptoTransactions.AddAsync(transaction);
                            await Context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while creating external investor.");
                            await _userManager.DeleteAsync(user);
                        }
                    }
                }
            }
        }

        private class ExternalInvestorDataRecord
        {
            public Guid Id { get; set; }
            public DateTime DateTime { get; set; }
            public decimal ETH { get; set; }
            public decimal BTC { get; set; }
        }
    }
}
