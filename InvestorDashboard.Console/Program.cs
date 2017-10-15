using AutoMapper;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;

namespace InvestorDashboard.Console
{
    internal static class Program
    {
        private static void Main()
        {
            var configurationBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", false, true)
              .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddAutoMapper(typeof(DependencyInjection));

            Configuration.Configure(serviceCollection, configuration);
            DependencyInjection.Configure(serviceCollection);

            var keyVaultService = serviceCollection
                .BuildServiceProvider()
                .GetRequiredService<IKeyVaultService>();
            keyVaultService.Initialize().Wait();

            serviceCollection.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(keyVaultService.DatabaseConnectionString, y => y.MigrationsAssembly("InvestorDashboard.Backend")));

            var serviceProvider = serviceCollection.BuildServiceProvider();

            RenewEthereumTransactions(serviceProvider);
        }

        private static void RenewEthereumTransactions(ServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var mapper = serviceProvider.GetRequiredService<IMapper>();
            var ethereumService = serviceProvider.GetRequiredService<IEthereumService>();
            var exchangeRateService = serviceProvider.GetRequiredService<IExchangeRateService>();

            var tokenRate = exchangeRateService.GetExchangeRate(Currency.DTT, Currency.USD);
            var ethRate = exchangeRateService.GetExchangeRate(Currency.ETH, Currency.USD);

            using (var ctx = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                foreach (var address in ctx.CryptoAddresses.Where(x => x.Currency == Currency.ETH && x.Type == AddressType.Investment))
                {
                    foreach (var transaction in ethereumService.GetInboundTransactionsByRecipientAddress(address.Address))
                    {
                        if (!ctx.Transactions.Any(x => x.Hash == transaction.Hash))
                        {
                            var trx = mapper.Map<Transaction>(transaction);
                            trx.Address = address;
                            trx.Currency = Currency.ETH;
                            trx.Direction = TransactionDirection.Inbound;
                            trx.ExchangeRate = ethRate;
                            trx.TokenPrice = tokenRate;
                            ctx.Transactions.Add(trx);
                        }
                    }
                }

                ctx.SaveChanges();
            }
        }
    }
}
