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
        private readonly IResourceService _resourceService;
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly ICalculationService _calculationService;
        private readonly ITokenService _tokenService;
        private readonly IGenericAddressService _genericAddressService;

        public ExternalInvestorService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IExchangeRateService exchangeRateService,
            IResourceService resourceService,
            IDashboardHistoryService dashboardHistoryService,
            ICalculationService calculationService,
            ITokenService tokenService,
            IGenericAddressService genericAddressService,
            IOptions<TokenSettings> tokenSettings)
            : base(serviceProvider, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
        }

        public async Task SynchronizeInvestorsData()
        {
            var records = _resourceService.GetCsvRecords<ExternalInvestorDataRecord>("ExternalInvestorData.csv");

            records = records
                .Where(x => x.DateTime < DateTime.UtcNow)
                .ToArray();

            foreach (var record in records)
            {
                using (var ctx = CreateContext())
                {
                    var user = ctx.Users.SingleOrDefault(x => x.ExternalId == record.Id);

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            Email = $"{record.Id}@{record.Id}.com",
                            UserName = record.Id.ToString(),
                            ExternalId = record.Id
                        };

                        var result = await _userManager.CreateAsync(user, "QAZwsxedc123");

                        if (result.Succeeded)
                        {
                            try
                            {
                                await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));
                                await _genericAddressService.CreateMissingAddresses(user.Id, false);

                                var address = ctx.CryptoAddresses.SingleOrDefault(x => x.UserId == user.Id && !x.IsDisabled && x.Currency == record.Currency);

                                if (address == null)
                                {
                                    Logger.LogWarning($"User {user.Id} doesn't have required {record.Currency} address.");
                                    await _userManager.DeleteAsync(user);
                                    return;
                                }

                                var value = _calculationService.ToStringValue(record.Value, record.Currency);

                                var transaction = new CryptoTransaction
                                {
                                    Amount = value,
                                    Direction = CryptoTransactionDirection.Inbound,
                                    CryptoAddressId = address.Id,
                                    Timestamp = record.DateTime
                                };

                                await ctx.CryptoTransactions.AddAsync(transaction);
                                await ctx.SaveChangesAsync();

                                await _tokenService.RefreshTokenBalance(user.Id);
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
        }

        private class ExternalInvestorDataRecord
        {
            public Guid Id { get; set; }
            public DateTime DateTime { get; set; }
            public Currency Currency { get; set; }
            public decimal Value { get; set; }
        }
    }
}
