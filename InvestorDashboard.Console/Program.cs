using AutoMapper;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            var options = serviceProvider.GetRequiredService<IOptions<ExchangeRateSettings>>();

            var eth = Task.Run(() => Run(serviceProvider, RenewEthereumTransactions, TimeSpan.FromHours(1), CancellationToken.None));
            var ex = Task.Run(() => Run(serviceProvider, RefreshExchangeRates, options.Value.RefreshRate, CancellationToken.None));
            Task.WaitAll(eth, ex);
        }

        private static void RenewEthereumTransactions(IServiceProvider serviceProvider)
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

            //using (var ctx = serviceProvider.GetRequiredService<ApplicationDbContext>())
            //{
            //    foreach (var address in ctx.CryptoAddresses.Where(x => x.Currency == Currency.ETH && x.Type == AddressType.Investment))
            //    {
            //        foreach (var transaction in ethereumService.GetInboundTransactionsByRecipientAddress(address.Address))
            //        {
            //            if (!ctx.Transactions.Any(x => x.Hash == transaction.Hash))
            //            {
            //                var trx = mapper.Map<CryptoTransaction>(transaction);
            //                trx.Address = address;
            //                trx.Direction = TransactionDirection.Inbound;
            //                trx.ExchangeRate = exchangeRateService.GetExchangeRate(Currency.ETH, Currency.USD, trx.Created, true);
            //                trx.TokenPrice = tokenRate;
            //                ctx.Transactions.Add(trx);
            //                ctx.SaveChanges();
            //            }
            //        }
            //    }
            //}
        }

        private static void RefreshExchangeRates(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var exchangeRateService = serviceProvider.GetRequiredService<IExchangeRateService>();

            foreach (var currency in new[] { Currency.ETH, Currency.BTC })
            {
                exchangeRateService.RefreshExchangeRate(currency);
            }
        }

        private static void Run(IServiceProvider serviceProvider, Action<IServiceProvider> action, TimeSpan period, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Task.Delay(period, cancellationToken).Wait();
                if (!cancellationToken.IsCancellationRequested)
                {
                    action(serviceProvider);
                }
            }
        }
    }
}
