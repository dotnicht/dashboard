using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class InvestorsService : ContextService, IInvestorsService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEnumerable<ICryptoService> _cryptoServices;
        private readonly IOptions<TokenSettings> _tokenSettings;
        private readonly IExchangeRateService _exchangeRateService;

        public InvestorsService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IEnumerable<ICryptoService> cryptoServices,
            IExchangeRateService exchangeRateService,
            IOptions<TokenSettings> tokenSettings)
            : base(context, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
        }

        public async Task<int> LoadInvestorsData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(GetType(), "InvestorsData.csv"))
            using (var reader = new StreamReader(stream))
            {
                var csv = new CsvReader(reader);
                var records = csv.GetRecords<InvestorRecord>().ToArray();
                Logger.LogDebug($"Total { records.Length } to be loaded.");
                var count = 0;
                foreach (var record in records)
                {
                    if (!Context.Users.Any(x => x.ExternalId == record.Id))
                    {
                        var email = $"{Guid.NewGuid()}@{Guid.NewGuid()}.com";
                        var user = new ApplicationUser
                        {
                            Email = email,
                            UserName = email,
                            ExternalId = record.Id,
                            ActivationDate = record.Day,
                            ExternalBitcoinInvestment = record.BTC,
                            ExternalEthereumInvestment = record.ETH
                        };

                        await _userManager.CreateAsync(user, "Us3g5!LrBFZ)E,G$");
                        Parallel.ForEach(_cryptoServices, async x => await x.UpdateUserDetails(user.Id));
                        user.ConfirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        await Context.SaveChangesAsync();

                        count++;
                    }
                }

                return count;
            }
        }

        public async Task<int> ActivateInvestors()
        {
            var ids = Context.Users
                .Where(x => x.ExternalId != null && x.ActivationDate <= DateTime.UtcNow && !x.EmailConfirmed)
                .Select(x => x.Id)
                .ToArray();

            Logger.LogDebug($"Total { ids.Length } users to be activated.");

            var count = 0;

            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);
                var result = await _userManager.ConfirmEmailAsync(user, user.ConfirmationCode);

                if (result.Succeeded)
                {
                    var currency = new Random().Next(0, 2) == 0
                        ? Currency.BTC
                        : Currency.ETH;

                    var transaction = new CryptoTransaction
                    {
                        Amount = currency == Currency.BTC
                            ? user.ExternalBitcoinInvestment.Value
                            : user.ExternalEthereumInvestment.Value,
                        Direction = CryptoTransactionDirection.Inbound,
                        ExchangeRate = await _exchangeRateService.GetExchangeRate(currency, Currency.USD, DateTime.UtcNow, true),
                        TokenPrice = _tokenSettings.Value.Price,
                        BonusPercentage = _tokenSettings.Value.BonusPercentage,
                        CryptoAddress = Context.CryptoAddresses.Single(x => x.UserId == id && x.Currency == currency && x.Type == CryptoAddressType.Investment && !x.IsDisabled),
                        TimeStamp = DateTime.UtcNow
                    };

                    await Context.CryptoTransactions.AddAsync(transaction);
                    await Context.SaveChangesAsync();

                    count++;
                }
            }

            return count;
        }

        public async Task<int> ClearInvestors()
        {
            var count = 0;

            foreach (var id in Context.Users.Where(x => x.ExternalId != null).Select(x => x.Id).ToArray())
            {
                var user = await _userManager.FindByIdAsync(id);
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    count++;
                }
            }

            return count;
        }

        private class InvestorRecord
        {
            public Guid Id { get; set; }
            public DateTime Day { get; set; }
            public decimal BTC { get; set; }
            public decimal ETH { get; set; }
        }
    }
}
