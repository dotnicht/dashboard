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
                    try
                    {
                        await RefreshTokenBalanceInternal(id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while refreshing balance for user {id}.");
                    }
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

        public async Task<(string Hash, bool Success)> Transfer(string userId, string destinationAddress, decimal amount)
        {
            if (destinationAddress == null)
            {
                throw new ArgumentNullException(nameof(destinationAddress));
            }

            if (_options.Value.IsTokenTransferDisabled)
            {
                Logger.LogWarning($"Token transfer globally disabled. User id {userId}.");
                return (Hash: null, Success: false);
            }

            if (!await IsUserEligibleForTransfer(userId))
            {
                Logger.LogWarning($"An attempt to perform an outbound transaction for non eligible user {userId}. Destination address {destinationAddress}. Amount {amount}.");
                return (Hash: null, Success: false);
            }

            var user = Context.Users.Include(x => x.CryptoAddresses).Single(x => x.Id == userId);

            var ethAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Investment && !x.IsDisabled && x.Currency == Currency.ETH);
            var dttAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Transfer && !x.IsDisabled && x.Currency == Currency.DTT);

            if (ethAddress == null || dttAddress == null)
            {
                Logger.LogError($"User {userId} has missing required addresses.");
                return (Hash: null, Success: false);
            }

            if (user.Balance + user.BonusBalance < amount)
            {
                throw new InvalidOperationException($"Insufficient token balance for user {userId} to perform transfer to {destinationAddress}. Amount {amount}.");
            }

            var balance = await _ethereumService.CallSmartContractBalanceOfFunction(ethAddress.Address);

            if (balance < amount)
            {
                throw new InvalidOperationException($"Actual smart contract balance is lower than requested transfer amount.");
            }

            var result = await _ethereumService.CallSmartContractTransferFromFunction(ethAddress, destinationAddress, amount);

            if (result.Success)
            {
                await Context.CryptoTransactions.AddAsync(new CryptoTransaction
                {
                    CryptoAddressId = dttAddress.Id,
                    Amount = amount,
                    TimeStamp = DateTime.UtcNow,
                    Direction = CryptoTransactionDirection.Outbound,
                    Hash = result.Hash,
                    TokenPrice = 1,
                    ExchangeRate = 1
                });

                await Context.SaveChangesAsync();
                await RefreshTokenBalanceInternal(userId);

                return result;
            }

            return (Hash: null, Success: false);
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
                    || (x.Direction == CryptoTransactionDirection.Internal && x.CryptoAddress.Type == CryptoAddressType.Internal && x.ExternalId != null));

            var balance = transactions.Sum(x => (x.Amount * x.ExchangeRate) / x.TokenPrice);
            var bonus = transactions.Sum(x => ((x.Amount * x.ExchangeRate) / x.TokenPrice) * (x.BonusPercentage / 100));

            var outbound = user.CryptoAddresses
                    .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.DTT && x.Type == CryptoAddressType.Transfer)
                    ?.CryptoTransactions
                    .Where(x => x.Direction == CryptoTransactionDirection.Outbound && x.Hash != null && !x.Failed)
                    ?.Sum(x => x.Amount)
                ?? 0;

            var tempBalance = balance;
            var tempBonus = bonus;

            if (balance < outbound)
            {
                bonus -= outbound - balance;
                balance = 0;

                if (bonus < 0)
                {
                    bonus = 0;
                    //TODO: remove hack and sync balance precisely.
                    //throw new InvalidOperationException($"Inconsistent balance detected for user {userId}. Balance: {tempBalance}. Bonus: {tempBonus}. Total: {tempBalance + tempBonus}. Outbound: {outbound}.");
                    Logger.LogError($"Inconsistent balance detected for user {userId}. Balance: {tempBalance}. Bonus: {tempBonus}. Total: {tempBalance + tempBonus}. Outbound: {outbound}.");
                }
            }
            else
            {
                balance -= outbound;
            }

            if (user.Balance != balance || user.BonusBalance != bonus)
            {
                user.Balance = balance;
                user.BonusBalance = bonus;

                // TODO: balance and bonus balance change notification.

                await Context.SaveChangesAsync();
            }

            var address = user.CryptoAddresses.Single(x => x.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment && !x.IsDisabled);
            var updated = user.Balance + user.BonusBalance;
            var external = await _ethereumService.CallSmartContractBalanceOfFunction(address.Address);

            if (updated != external)
            {
                Logger.LogError($"Balance at smart contract is incosistent with database for user {userId}. Smart contract balance: {updated}. Database balance: {external}.");
            }
        }
    }
}
