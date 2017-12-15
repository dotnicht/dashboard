using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
        private readonly IEthereumService _ethereumService;

        public ExternalInvestorService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IExchangeRateService exchangeRateService,
            ICsvService csvService,
            IEthereumService ethereumService,
            IOptions<TokenSettings> tokenSettings)
            : base(context, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
            _ethereumService = ethereumService ?? throw new ArgumentNullException(nameof(ethereumService));
        }

        public async Task SynchronizeInvestorsData()
        {
            var records = _csvService.GetRecords<ExternalInvestorDataRecord>("ExternalInvestorData.csv")
                .Where(x => x.Day < DateTime.UtcNow)
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
                        var address = await _ethereumService.CreateCryptoAddress(user.Id);

                        // TODO: extract transaction props from history.
                        var transaction = new CryptoTransaction
                        {
                            Amount = record.ETH,
                            Direction = CryptoTransactionDirection.Inbound,
                            ExchangeRate = await _exchangeRateService.GetExchangeRate(Currency.ETH, Currency.USD, record.Day, true),
                            TokenPrice = _tokenSettings.Value.Price,
                            BonusPercentage = _tokenSettings.Value.BonusPercentage,
                            CryptoAddressId = address.Id,
                            TimeStamp = record.Day
                        };

                        await Context.CryptoTransactions.AddAsync(transaction);
                    }
                }
            }

            await Context.SaveChangesAsync();
        }

        private class ExternalInvestorDataRecord
        {
            public Guid Id { get; set; }
            public DateTime Day { get; set; }
            public decimal BTC { get; set; }
            public decimal ETH { get; set; }
        }
    }
}
