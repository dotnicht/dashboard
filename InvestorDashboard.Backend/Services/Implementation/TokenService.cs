using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ContextService, ITokenService
    {
        private readonly IEthereumService _ethereumService;
        private readonly IOptions<TokenSettings> _options;

        public TokenService(ApplicationDbContext context, ILoggerFactory loggerFactory, IEthereumService ethereumService, IOptions<TokenSettings> options)
            : base(context, loggerFactory)
        {
            _ethereumService = ethereumService ?? throw new ArgumentNullException(nameof(ethereumService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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

        public async Task<bool> IsUserEligibleForTransfer(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return await Context.CryptoTransactions.Where(x => x.CryptoAddress.UserId == userId && !x.CryptoAddress.IsDisabled && x.CryptoAddress.Currency == Currency.DTT)
                .CountAsync() < _options.Value.OutboundTransactionsLimit;
        }

        public async Task<bool> Transfer(string userId, string destinationAddress, decimal amount)
        {
            if (destinationAddress == null)
            {
                throw new ArgumentNullException(nameof(destinationAddress));
            }

            if (_options.Value.IsTokenTransferDisabled)
            {
                Logger.LogWarning($"Token transfer globally disabled. User id {userId}.");
                return false;
            }

            if (!await IsUserEligibleForTransfer(userId))
            {
                Logger.LogWarning($"An attempt to perform an outbound transaction for non eligible user {userId}. Destination address {destinationAddress}. Amount {amount}.");
                return false;
            }

            var user = Context.Users.Include(x => x.CryptoAddresses).Single(x => x.Id == userId);

            var ethAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Investment && !x.IsDisabled && x.Currency == Currency.ETH);
            var dttAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Transfer && !x.IsDisabled && x.Currency == Currency.DTT);

            if (ethAddress == null || dttAddress == null)
            {
                Logger.LogError($"User {userId} has missing requirted addresses.");
                return false;
            }

            if (user.Balance + user.BonusBalance < amount)
            {
                throw new InvalidOperationException($"Insufficient token balance for user {userId} to perform transfer to {destinationAddress}. Amount {amount}.");
            }

            if ((await _ethereumService.CallSmartContractTransferFromFunction(ethAddress, destinationAddress, amount)).Success)
            {
                await Context.CryptoTransactions.AddAsync(new CryptoTransaction
                {
                    CryptoAddressId = dttAddress.Id,
                    Amount = amount,
                    TimeStamp = DateTime.UtcNow,
                    Direction = CryptoTransactionDirection.Outbound
                });

                await Context.SaveChangesAsync();
                await RefreshTokenBalanceInternal(userId);

                return true;
            }

            return false;
        }

        private async Task RefreshTokenBalanceInternal(string userId)
        {
            var user = Context.Users
                .Include(x => x.CryptoAddresses)
                .ThenInclude(x => x.CryptoTransactions)
                .SingleOrDefault(x => x.Id == userId);

            if (user == null)
            {
                throw new InvalidOperationException($"User not found with ID {userId}.");
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

            var balance = transactions.Sum(x => (x.Amount * x.ExchangeRate) / x.TokenPrice);
            var bonus = transactions.Sum(x => ((x.Amount * x.ExchangeRate) / x.TokenPrice) * (x.BonusPercentage / 100));

            var outbound = user.CryptoAddresses
                    .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.DTT && x.Type == CryptoAddressType.Transfer)
                    ?.CryptoTransactions
                    ?.Sum(x => x.Amount)
                ?? 0;

            if (balance < outbound)
            {
                bonus -= outbound - balance;
                balance = 0;

                if (bonus < 0)
                {
                    throw new InvalidOperationException($"Inconsistent balance detected for user {userId}.");
                }
            }
            else
            {
                balance = balance - outbound;
            }

            if (user.Balance != balance)
            {
                user.Balance = balance;
            }

            if (user.BonusBalance != bonus)
            {
                user.BonusBalance = bonus;
            }

            // TODO: balance and bonus balance change notification.

            await Context.SaveChangesAsync();
        }
    }
}
