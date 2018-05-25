using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.CSharpVitamins;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class ReferralService : ContextService, IReferralService
    {
        private readonly ICalculationService _calculationService;
        private readonly IOptions<ReferralSettings> _options;

        public ReferralService(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, ICalculationService calculationService, IOptions<ReferralSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<(Dictionary<string, decimal> Transactions, decimal Pending, decimal Balance)> GetRererralData(string userId, Currency currency)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            using (var ctx = CreateContext())
            {
                var referral = ctx.CryptoTransactions
                    .Where(x => x.Direction == CryptoTransactionDirection.Referral && x.CryptoAddress.Currency == currency && x.CryptoAddress.UserId == userId)
                    .ToDictionaryAsync(x => x.Hash, x => _calculationService.ToDecimalValue(x.Amount, currency));

                var tx = ctx.CryptoTransactions
                    .Where(x => x.CryptoAddress.Currency == currency && x.CryptoAddress.User.ReferralUserId == userId);

                var pending = tx.Where(x => x.IsReferralPaid == false)
                    .Select(x => _calculationService.ToDecimalValue(x.Amount, currency))
                    .SumAsync();

                var balance = tx
                    .Select(x => _calculationService.ToDecimalValue(x.Amount, currency))
                    .SumAsync();

                return (Transactions: await referral, Pending: await pending * _options.Value.Reward, Balance: await balance * _options.Value.Reward);
            }
        }

        public async Task PopulateReferralData(string userId, string referralCode)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            using (var ctx = CreateContext())
            {
                var user = ctx.Users
                    .SingleOrDefault(x => x.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException($"User not found with ID {userId}.");
                }

                user.ReferralCode = ShortGuid.NewGuid().ToString();

                if (!string.IsNullOrWhiteSpace(referralCode))
                {
                    user.ReferralUserId = ctx.Users
                        .SingleOrDefault(x => x.ReferralCode == referralCode)
                        ?.Id;
                }

                await ctx.SaveChangesAsync();
            }
        }
    }
}
