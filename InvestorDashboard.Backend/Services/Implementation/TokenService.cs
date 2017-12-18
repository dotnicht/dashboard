using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ContextService, ITokenService
    {
        public TokenService(ApplicationDbContext context, ILoggerFactory loggerFactory)
            : base(context, loggerFactory)
        {
        }

        public async Task RefreshTokenBalance(string userId = null)
        {
            if (userId == null)
            {
                foreach (var id in Context.Users.Select(x => x.Id))
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
            var user = Context.Users
                .Include(x => x.CryptoAddresses)
                .ThenInclude(x => x.CryptoTransactions)
                .SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID { userId }.");
            }

            var transactions = user.CryptoAddresses
                .Where(
                    x => (x.Type == CryptoAddressType.Investment && x.Currency != Currency.DTT) 
                    || (x.Type == CryptoAddressType.Internal && x.Currency == Currency.DTT))
                .SelectMany(x => x.CryptoTransactions)
                .Where(
                    x => (x.Direction == CryptoTransactionDirection.Inbound && x.CryptoAddress.Type == CryptoAddressType.Investment) 
                    || (x.Direction == CryptoTransactionDirection.Internal && x.CryptoAddress.Type == CryptoAddressType.Internal && x.ExternalId != null))
                .ToArray();

            user.Balance = transactions.Sum(x => (x.Amount * x.ExchangeRate) / x.TokenPrice);
            user.BonusBalance = transactions.Sum(x => ((x.Amount * x.ExchangeRate) / x.TokenPrice) * (x.BonusPercentage / 100));

            await Context.SaveChangesAsync();
        }
    }
}
