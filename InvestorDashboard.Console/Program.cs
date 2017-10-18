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
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;

namespace InvestorDashboard.Console
{
    internal static class Program
    {
        private static void Main()
        {
            Run().GetAwaiter().GetResult();
        }

        private static async Task Run()
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

            await keyVaultService.Initialize().ConfigureAwait(false);

            serviceCollection.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(keyVaultService.DatabaseConnectionString, y => y.MigrationsAssembly("InvestorDashboard.Backend")));

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var schedulerFactory = new StdSchedulerFactory(new NameValueCollection { { "quartz.serializer.type", "binary" } });
            var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);

            await scheduler.Start().ConfigureAwait(false);

            WriteLine("Press the escape key (ESC) to quit.");
            while (ReadKey(true).Key != ConsoleKey.Escape)
            {
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }

            await scheduler.Shutdown().ConfigureAwait(false);
        }
    }
}
