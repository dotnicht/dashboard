using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ExternalInvestorService : ContextService, IExternalInvestorService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICsvService _csvService;

        public ExternalInvestorService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IEnumerable<ICryptoService> cryptoServices,
            IExchangeRateService exchangeRateService,
            ICsvService csvService,
            IOptions<TokenSettings> tokenSettings)
            : base(context, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
        }

        public async Task<int> LoadInvestorsData()
        {
            var count = 0;

            foreach (var record in _csvService.GetRecords<ExternalInvestorDataRecord>("ExternalInvestorData.csv"))
            {
                if (!Context.Users.Any(x => x.ExternalId == record.Id))
                {
                    var email = $"{Guid.NewGuid()}@{Guid.NewGuid()}.com";

                    var user = new ApplicationUser
                    {
                        Email = email,
                        UserName = email,
                        ExternalId = record.Id
                    };

                    await _userManager.CreateAsync(user, "Us3g5!LrBFZ)E,G$");

                    Parallel.ForEach(_cryptoServices, async x => await x.CreateCryptoAddress(user.Id));

                    count++;
                }
            }

            return count;
        }

        public async Task SynchronizeInvestorsData()
        {
            var users = Context.Users
                .Where(x => x.ExternalId != null && !x.EmailConfirmed)
                .ToArray()
                .Join(_csvService.GetRecords<ExternalInvestorDataRecord>("InvestorsData.csv"), x => x.ExternalId, x => x.Id, (x, y) => new { User = x, Record = y })
                .Where(x => x.Record.Day <= DateTime.UtcNow)
                .ToArray();

            Logger.LogDebug($"Total { users.Length } users to be activated.");

            foreach (var user in users)
            {
                var managedUser = await _userManager.FindByIdAsync(user.User.Id);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(managedUser);
                var result = await _userManager.ConfirmEmailAsync(managedUser, code);

                if (result.Succeeded)
                {
                    var services = _cryptoServices
                        .Where(x => !x.Settings.Value.IsDisabled)
                        .ToArray();

                    if (services.Length == 0)
                    {
                        throw new InvalidOperationException("At least one crypto currency should be enabled.");
                    }

                    var currency = services[new Random().Next(0, services.Length)].Settings.Value.Currency;

                    var address = Context.CryptoAddresses
                        .SingleOrDefault(x => x.UserId == user.User.Id && x.Currency == currency && x.Type == CryptoAddressType.Investment && !x.IsDisabled);

                    if (address == null)
                    {
                        throw new InvalidOperationException($"The enabled investment {currency} address for user {user.User.Id} was not found.");
                    }

                    var transaction = new CryptoTransaction
                    {
                        Amount = currency == Currency.BTC
                            ? user.Record.BTC
                            : user.Record.ETH,
                        Direction = CryptoTransactionDirection.Inbound,
                        ExchangeRate = await _exchangeRateService.GetExchangeRate(currency, Currency.USD, DateTime.UtcNow, true),
                        TokenPrice = _tokenSettings.Value.Price,
                        BonusPercentage = _tokenSettings.Value.BonusPercentage,
                        CryptoAddress = address,
                        TimeStamp = DateTime.UtcNow
                    };

                    await Context.CryptoTransactions.AddAsync(transaction);
                    await Context.SaveChangesAsync();

                }
            }
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
