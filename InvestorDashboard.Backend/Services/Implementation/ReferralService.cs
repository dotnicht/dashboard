using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.CSharpVitamins;
using InvestorDashboard.Backend.Database;
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

        public ReferralService(ApplicationDbContext context, ILoggerFactory loggerFactory, ICalculationService calculationService, IOptions<ReferralSettings> options)
            : base(context, loggerFactory)
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

            var referral = Context.CryptoTransactions
                .Where(x => x.Direction == CryptoTransactionDirection.Referral && x.CryptoAddress.Currency == currency && x.CryptoAddress.UserId == userId)
                .ToDictionaryAsync(x => x.Hash, x => _calculationService.ToDecimalValue(x.Amount, currency));

            var tx = Context.CryptoTransactions
                .Where(x => x.CryptoAddress.Currency == currency && x.CryptoAddress.User.ReferralUserId == userId);

            var pending = await tx.Where(x => x.IsReferralPaid == false)
                .Select(x => _calculationService.ToDecimalValue(x.Amount, currency))
                .SumAsync();

            var balance = await tx
                .Select(x => _calculationService.ToDecimalValue(x.Amount, currency))
                .SumAsync();

            return (Transactions: await referral, Pending: pending * _options.Value.Reward, Balance: balance * _options.Value.Reward);
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
                entity.IsDisabled = entity.Address != address;
            }

            if (!string.IsNullOrWhiteSpace(address) && (entity == null || entity.IsDisabled))
            {
                Context.CryptoAddresses.Add(new CryptoAddress { Currency = currency, UserId = user.Id, Address = address, Type = CryptoAddressType.Referral });
            }

            await Context.SaveChangesAsync();
        }
    }
}
