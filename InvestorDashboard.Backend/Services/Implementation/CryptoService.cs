using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using Microsoft.Extensions.Options;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class CryptoService : ContextService, ICryptoService
    {
        public abstract Currency Currency { get; }
        protected IExchangeRateService ExchangeRateService { get; }
        protected IKeyVaultService KeyVaultService { get; }
        protected IEmailService EmailService { get; }
        protected IMapper Mapper { get; }
        protected IOptions<TokenSettings> TokenSettings { get; }

        protected CryptoService(ApplicationDbContext context, IExchangeRateService exchangeRateService, IKeyVaultService keyVaultService, IEmailService emailService, IMapper mapper, IOptions<TokenSettings> tokenSettings)
            : base(context)
        {
            ExchangeRateService = exchangeRateService ?? throw new ArgumentNullException(nameof(exchangeRateService));
            KeyVaultService = keyVaultService ?? throw new ArgumentNullException(nameof(keyVaultService));
            EmailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            TokenSettings = tokenSettings ?? throw new ArgumentNullException(nameof(tokenSettings));
        }

        public Task UpdateUserDetails(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return UpdateUserDetailsInternal(userId);
        }

        public async Task RefreshInboundTransactions()
        {
            var hashes = Context.CryptoTransactions.Select(x => x.Hash).ToHashSet();

            foreach (var address in Context.CryptoAddresses.Where(x => x.Currency == Currency && x.Type == CryptoAddressType.Investment).ToArray())
            {
                foreach (var transaction in await GetTransactionsFromBlockchain(address.Address))
                {
                    if (!hashes.Contains(transaction.Hash))
                    {
                        transaction.CryptoAddress = address;
                        transaction.Direction = CryptoTransactionDirection.Inbound;
                        transaction.ExchangeRate = await ExchangeRateService.GetExchangeRate(Currency, Currency.USD, transaction.TimeStamp, true);
                        transaction.TokenPrice = TokenSettings.Value.Price;
                        transaction.BonusPercentage = TokenSettings.Value.BonusPercentage;

                        Context.CryptoTransactions.Add(transaction);
                        Context.SaveChanges();

                        // TODO: send transaction confirmed email.
                    }
                }
            }
        }

        protected abstract Task UpdateUserDetailsInternal(string userId);

        protected abstract Task<IEnumerable<CryptoTransaction>> GetTransactionsFromBlockchain(string address);
    }
}
