using InvestorDashboard.Backend.CSharpVitamins;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ReferralService : ContextService, IReferralService
    {
        private readonly ICalculationService _calculationService;

        public ReferralService(ApplicationDbContext context, ILoggerFactory loggerFactory, ICalculationService calculationService)
            : base(context, loggerFactory)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
        }

        public async Task<(IDictionary<string, decimal> Transactions, decimal Pending)> GetRererralData(string userId, Currency currency)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var tx = Context.CryptoTransactions
                .Where(x => x.Direction == CryptoTransactionDirection.Referral && x.CryptoAddress.Currency == currency && x.CryptoAddress.UserId == userId)
                .ToDictionaryAsync(x => x.Hash, x => _calculationService.ToDecimalValue(x.Amount, currency));

            var pending = Context.CryptoTransactions
                .Where(x => x.CryptoAddress.Currency == currency && x.CryptoAddress.User.ReferralUserId == userId && x.IsReferralPaid == false)
                .Select(x => _calculationService.ToDecimalValue(x.Amount, currency))
                .SumAsync();

            return (Transactions: await tx, Pending: await pending * 0.05m);
        }

        public async Task PopulateReferralData(ApplicationUser user, string referralCode)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.ReferralCode = ShortGuid.NewGuid().ToString();

            if (!string.IsNullOrWhiteSpace(referralCode))
            {
                user.ReferralUserId = Context.Users.SingleOrDefault(x => x.ReferralCode == referralCode)?.Id;
            }

            await Context.SaveChangesAsync();
        }

        public async Task UpdateReferralAddress(ApplicationUser user, Currency currency, string address)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var entity = user.CryptoAddresses.SingleOrDefault(x => x.Currency == currency && x.Type == CryptoAddressType.Referral && !x.IsDisabled);

            if (entity != null)
            {
                entity.IsDisabled = true;
            }

            if (!string.IsNullOrWhiteSpace(address))
            {
                Context.CryptoAddresses.Add(new CryptoAddress { Currency = currency, UserId = user.Id, Address = address });
            }

            await Context.SaveChangesAsync();
        }
    }
}
