using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;

        public TokenService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<decimal> RefreshTokenBalance(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = _context.Users.SingleOrDefault(x => x.Id == userId);
            if (user == null)

            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
            }

            var balance = _context.CryptoAccounts
                .Where(x => x.UserId == userId)
                .SelectMany(x => x.CryptoAddresses)
                .Where(x => x.Type == CryptoAddressType.Investment)
                .SelectMany(x => x.CryptoTransactions)
                .Where(x => x.Direction == CryptoTransactionDirection.Inbound)
                .Sum(x => x.Amount * x.ExchangeRate / x.TokenPrice);

            user.Balance = balance;
            await _context.SaveChangesAsync();

            return balance; 
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
