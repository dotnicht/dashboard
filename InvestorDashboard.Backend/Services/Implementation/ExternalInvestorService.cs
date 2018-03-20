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
        private readonly IResourceService _resourceService;
        private readonly IDashboardHistoryService _dashboardHistoryService;
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public ExternalInvestorService(
            ApplicationDbContext context,
            ILoggerFactory loggerFactory,
            UserManager<ApplicationUser> userManager,
            IExchangeRateService exchangeRateService,
            IResourceService resourceService,
            IDashboardHistoryService dashboardHistoryService,
            IEnumerable<ICryptoService> cryptoServices,
            IOptions<TokenSettings> tokenSettings)
            : base(context, loggerFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _tokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _dashboardHistoryService = dashboardHistoryService ?? throw new ArgumentNullException(nameof(dashboardHistoryService));
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        public async Task SynchronizeInvestorsData()
        {
            var records = _resourceService.GetCsvRecords<ExternalInvestorDataRecord>("ExternalInvestorData.csv")
                .Where(x => x.DateTime < DateTime.UtcNow)
                .ToArray();

            foreach (var record in records)
            {
                if (!Context.Users.Any(x => x.ExternalId == record.Id))
                {
                    var user = new ApplicationUser
                    {
                        Email = $"{record.Id}@{record.Id}.com",
                        UserName = record.Id.ToString(),
                        ExternalId = record.Id
                    };

                    var result = await _userManager.CreateAsync(user);

                    if (result.Succeeded)
                    {
                        try
                        {
                            var service = _cryptoServices.Single(x => x.Settings.Value.Currency == record.Currency);
                            var address = await service.CreateCryptoAddress(user.Id);
                            var value = service.ToStringValue(record.Value);

                            var transaction = new CryptoTransaction
                            {
                                Amount = value,
                                Direction = CryptoTransactionDirection.Inbound,
                                CryptoAddressId = address.Id,
                                Timestamp = record.DateTime
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
            public Currency Currency { get; set; }
            public decimal Value { get; set; }
        }
    }
}
