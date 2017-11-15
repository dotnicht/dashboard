using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Logging;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ContextService, ITokenService
    {
        private readonly IAffiliatesService _affiliatesService;

        public TokenService(ApplicationDbContext context, ILoggerFactory loggerFactory, IAffiliatesService affiliatesService)
            : base(context, loggerFactory)
        {
            _affiliatesService = affiliatesService ?? throw new ArgumentNullException(nameof(affiliatesService));
        }

        public async Task RefreshTokenBalance(string userId = null)
        {
            if (userId == null)
            {
                foreach (var id in Context.Users.Where(x => x.ExternalId == null).Select(x => x.Id))
                {
                    await RefreshTokenBalanceInternal(id);
                }
            }
            else
            {
                await RefreshTokenBalanceInternal(userId);
            }
        }

        private async Task RefreshTokenBalanceInternal(string userId)
        {
            var user = Context.Users.SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
            }

            var transactions = Context.CryptoAddresses
                .Where(x => x.UserId == userId && x.Type == CryptoAddressType.Investment)
                .SelectMany(x => x.CryptoTransactions)
                .Where(x => x.Direction == CryptoTransactionDirection.Inbound)
                .ToArray();

            var affiliateBalance = await _affiliatesService.GetUserAffilicateBalance(userId);
            user.Balance = transactions.Sum(x => (x.Amount * x.ExchangeRate) / x.TokenPrice) + affiliateBalance;
            user.BonusBalance = transactions.Sum(x => ((x.Amount * x.ExchangeRate) / x.TokenPrice) * (x.BonusPercentage / 100));

            await Context.SaveChangesAsync();
        }
    }
}
