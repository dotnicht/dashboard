﻿using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class TokenService : ContextService, ITokenService
    {
        private readonly ISmartContractService _smartContractService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ICalculationService _calculationService;
        private readonly IOptions<TokenSettings> _options;

        public TokenService(
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            ISmartContractService smartContractService,
            IExchangeRateService exchangeRateService,
            ICalculationService calculationService,
            IOptions<TokenSettings> options)
            : base(serviceProvider, loggerFactory)
        {
            _smartContractService = smartContractService ?? throw new ArgumentNullException(nameof(smartContractService));
            _exchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task RefreshTokenBalance(string userId = null)
        {
            if (userId == null)
            {
                using (var ctx = CreateContext())
                {
                    var ids = ctx.Users
                        .Where(x => x.ExternalId == null && x.EmailConfirmed)
                        .Select(x => x.Id)
                        .ToArray();

                    foreach (var id in ids)
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

            using (var ctx = CreateContext())
            {
                var user = ctx.Users.Single(x => x.Id == userId);

                var count = await ctx.CryptoTransactions
                    .Where(x => x.CryptoAddress.UserId == userId && !x.CryptoAddress.IsDisabled && x.CryptoAddress.Currency == Currency.Token && x.Direction == CryptoTransactionDirection.Outbound)
                    .CountAsync();

                return user.IsEligibleForTransfer && count <= _options.Value.OutboundTransactionsLimit;
            }
        }

        public async Task<(string Hash, bool Success)> Transfer(string userId, string destinationAddress, BigInteger amount)
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

            using (var ctx = CreateContext())
            {
                await RefreshTokenBalanceInternal(userId);

                var user = ctx.Users.Include(x => x.CryptoAddresses).Single(x => x.Id == userId);

                var ethAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Investment && !x.IsDisabled && x.Currency == Currency.ETH);
                var tokenAddress = user.CryptoAddresses.SingleOrDefault(x => x.Type == CryptoAddressType.Internal && !x.IsDisabled && x.Currency == Currency.Token);

                if (ethAddress == null || tokenAddress == null)
                {
                    Logger.LogError($"User {userId} has missing required addresses.");
                    return (Hash: null, Success: false);
                }

                var tx = new CryptoTransaction
                {
                    CryptoAddressId = tokenAddress.Id,
                    Amount = amount.ToString(),
                    Timestamp = DateTime.UtcNow,
                    Direction = CryptoTransactionDirection.Outbound
                };

                ctx.CryptoTransactions.Add(tx);
                ctx.SaveChanges();

                await RefreshTokenBalance(userId);

                using (var context = CreateContext())
                {
                    user = context.Users.Include(x => x.CryptoAddresses).Single(x => x.Id == userId);
                }

                if (user.Balance + user.BonusBalance < amount)
                {
                    throw new InvalidOperationException($"Insufficient token balance for user {userId} to perform transfer to {destinationAddress}. Amount {amount}.");
                }

                if (_options.Value.IsDirectMintingDisabled)
                {
                    var balance = await _smartContractService.CallSmartContractBalanceOfFunction(ethAddress.Address);

                    if (balance < amount)
                    {
                        throw new InvalidOperationException($"Actual smart contract balance is lower than requested transfer amount.");
                    }
                }

                var result = _options.Value.IsDirectMintingDisabled
                    ? await _smartContractService.CallSmartContractTransferFromFunction(ethAddress, destinationAddress, amount)
                    : await _smartContractService.CallSmartContractMintTokensFunction(destinationAddress, amount);

                if (result.Success)
                {
                    tx.Hash = result.Hash;
                    ctx.SaveChanges();

                    await RefreshTokenBalanceInternal(userId);

                    return result;
                }
                else
                {
                    tx.IsFailed = true;
                    ctx.SaveChanges();
                }
            }

            return (Hash: null, Success: false);
        }

        private async Task RefreshTokenBalanceInternal(string userId)
        {
            using (var ctx = CreateContext())
            {
                var user = ctx.Users
                    .Include(x => x.CryptoAddresses)
                    .ThenInclude(x => x.CryptoTransactions)
                    .SingleOrDefault(x => x.Id == userId);

                if (user == null)
                {
                    throw new InvalidOperationException($"User not found with ID {userId}.");
                }

                var inboundTx = user.CryptoAddresses.Where(x => x.Type == CryptoAddressType.Investment && x.Currency != Currency.Token)
                    .SelectMany(x => x.CryptoTransactions)
                    .Where(x => x.Direction == CryptoTransactionDirection.Inbound && x.CryptoAddress.Type == CryptoAddressType.Investment)
                    .ToArray();

                var balance = 0L;

                foreach (var tx in inboundTx)
                {
                    var ex = await _exchangeRateService.GetExchangeRate(tx.CryptoAddress.Currency, _options.Value.Currency, tx.Timestamp);
                    balance += (long)Math.Ceiling(_calculationService.ToDecimalValue(tx.Amount, tx.CryptoAddress.Currency) * ex / _options.Value.Price);
                }

                var bonus = 0L;

                if (_options.Value.Bonus.System == TokenSettings.BonusSettings.BonusSystem.Schedule)
                {
                    // TODO: reimplement schedule bonus system.
                }
                else if (_options.Value.Bonus.System == TokenSettings.BonusSettings.BonusSystem.Percentage)
                {
                    foreach (var item in _options.Value.Bonus.Percentage)
                    {
                        if ((item.Lower == null || balance >= item.Lower) && (item.Upper == null || balance < item.Upper))
                        {
                            bonus = (long)Math.Ceiling(balance * item.Amount);
                            break;
                        }
                    }
                }

                bonus += user.CryptoAddresses
                    .Where(x => x.Type == CryptoAddressType.Internal && x.Currency == Currency.Token)
                    .SelectMany(x => x.CryptoTransactions)
                    .Where(x => x.Direction == CryptoTransactionDirection.Internal && x.CryptoAddress.Type == CryptoAddressType.Internal && !x.IsInactive)
                    .ToArray()
                    .Sum(x => long.Parse(x.Amount));

                var outbound = user.CryptoAddresses
                        .SingleOrDefault(x => !x.IsDisabled && x.Currency == Currency.Token && x.Type == CryptoAddressType.Internal)
                        ?.CryptoTransactions
                        ?.Where(x => x.Direction == CryptoTransactionDirection.Outbound && x.Hash != null && (x.IsFailed == null || !x.IsFailed.Value))
                        ?.ToArray()
                        ?.Sum(x => long.Parse(x.Amount))
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

                    await ctx.SaveChangesAsync();
                }

                if (_options.Value.AutomaticallyEnableTokenTransfer)
                {
                    var address = user.CryptoAddresses.Single(x => x.Currency == Currency.ETH && x.Type == CryptoAddressType.Investment && !x.IsDisabled);
                    var updated = user.Balance + user.BonusBalance;
                    var external = await _smartContractService.CallSmartContractBalanceOfFunction(address.Address);

                    if (external != 0)
                    {
                        if (updated != external && user.ExternalId == null)
                        {
                            Logger.LogError($"Balance at smart contract is incosistent with database for user {userId}. Smart contract balance: {external}. Database balance: {updated}.");
                            user.IsEligibleForTransfer = false;
                        }
                        else if (!user.IsEligibleForTransfer)
                        {
                            user.IsEligibleForTransfer = true;
                        }

                        await ctx.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
