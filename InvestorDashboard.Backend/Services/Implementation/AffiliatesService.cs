﻿using System;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class AffiliatesService : ContextService, IAffiliatesService
    {
        private readonly ICsvService _csvService;
        private readonly IOptions<TokenSettings> _options;

        public AffiliatesService(ApplicationDbContext context, ILoggerFactory loggerFactory, ICsvService csvService, IOptions<TokenSettings> options)
            : base(context, loggerFactory)
        {
            _csvService = csvService ?? throw new ArgumentNullException(nameof(csvService));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task SyncAffiliates()
        {
            foreach (var record in _csvService.GetRecords<AffiliatesRecord>("AffiliatesData.csv"))
            {
                if (!Context.CryptoTransactions.Any(x => x.ExternalId == record.Guid))
                {
                    try
                    {
                        var user = Context.Users
                            .Include(x => x.CryptoAddresses)
                            .SingleOrDefault(x => x.Email == record.Email);

                        if (user == null)
                        {
                            throw new InvalidOperationException($"User not found with email { record.Email }.");
                        }

                        var address = user.CryptoAddresses.SingleOrDefault(x => x.Currency == Currency.DTT)
                            ?? Context.CryptoAddresses.Add(new CryptoAddress { User = user, Currency = Currency.DTT, Type = CryptoAddressType.Internal }).Entity;

                        Context.CryptoTransactions.Add(new CryptoTransaction
                        {
                            Amount = record.DTT,
                            ExternalId = record.Guid,
                            CryptoAddress = address,
                            TokenPrice = _options.Value.Price,
                            Direction = CryptoTransactionDirection.Internal
                        });

                        await Context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurred while processing record { record.Guid }.");
                    }
                }
            }
        }

        public async Task<decimal> GetUserAffilicateBalance(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return Context.Users
                    .Include(x => x.CryptoAddresses)
                    .ThenInclude(x => x.CryptoTransactions)
                    .SingleOrDefault(x => x.Id == userId)
                    ?.CryptoAddresses
                    .SingleOrDefault(x => !x.IsDisabled && x.Type == CryptoAddressType.Internal && x.Currency == Currency.DTT)
                    ?.CryptoTransactions
                    .Where(x => x.Direction == CryptoTransactionDirection.Internal && x.ExternalId != null)
                    .Sum(x => x.Amount)
                ?? 0;
        }

        private class AffiliatesRecord
        {
            public Guid Guid { get; set; }
            public string Email { get; set; }
            public decimal DTT { get; set; }
        }
    }
}
