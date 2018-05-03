using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class InternalUserService : ContextService, IInternalUserService
    {
        private readonly IResourceService _resourceService;
        private readonly ITokenService _tokenService;
        private readonly IGenericAddressService _genericAddressService;

        private readonly IOptions<TokenSettings> _options;

        public InternalUserService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            IResourceService resourceService,
            ITokenService tokenService,
            IGenericAddressService genericAddressService,
            IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _resourceService = resourceService ?? throw new ArgumentNullException(nameof(resourceService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _genericAddressService = genericAddressService ?? throw new ArgumentNullException(nameof(genericAddressService));
        }

        public async Task SynchronizeInternalUsersData()
        {
            using (var ctx = CreateContext())
            {
                foreach (var record in _resourceService.GetCsvRecords<InternalUserDataRecord>("InternalUserData.csv"))
                {
                    if (!ctx.CryptoTransactions.Any(x => x.ExternalId == record.Guid))
                    {
                        try
                        {
                            var user = ctx.Users
                                .Include(x => x.CryptoAddresses)
                                .SingleOrDefault(x => x.Email == record.Email);

                            if (user == null)
                            {
                                throw new InvalidOperationException($"User not found with email {record.Email}.");
                            }

                            var address = await _genericAddressService.EnsureInternalAddress(user);

                            var tx = new CryptoTransaction
                            {
                                Amount = record.Tokens.ToString(),
                                ExternalId = record.Guid,
                                CryptoAddressId = address.Id,
                                Direction = CryptoTransactionDirection.Internal,
                                Timestamp = DateTime.UtcNow
                            };

                            await ctx.CryptoTransactions.AddAsync(tx);
                            await ctx.SaveChangesAsync();

                            await _tokenService.RefreshTokenBalance(user.Id);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, $"An error occurred while processing record {record.Guid}.");
                        }
                    }
                }
            }
        }

        private class InternalUserDataRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public long Tokens { get; set; }
        }
    }
}