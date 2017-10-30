using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ContextService, ITokenService
    {
        public TokenService(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<decimal> RefreshTokenBalance(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = Context.Users.SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
            }

            user.Balance = Context.CryptoAddresses
                .Where(x => x.UserId == userId && x.Type == CryptoAddressType.Investment)
                .SelectMany(x => x.CryptoTransactions)
                .Where(x => x.Direction == CryptoTransactionDirection.Inbound)
                .Sum(x => (x.Amount * x.ExchangeRate / x.TokenPrice) * (1 + (x.BonusPercentage / 100)));

            await Context.SaveChangesAsync();

            return user.Balance;
        }
    }
}
