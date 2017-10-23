using AutoMapper;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Console.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using static System.Console;

namespace InvestorDashboard.Console
{
    internal static class Program
    {
        // TODO: refactor this.
        public static ServiceProvider ServiceProvider { get; private set; }

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

            serviceCollection.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(keyVaultService.DatabaseConnectionString, y => y.MigrationsAssembly("InvestorDashboard.Backend")), ServiceLifetime.Transient);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var ctx = ServiceProvider.GetRequiredService<ApplicationDbContext>();


            var schedulerFactory = new StdSchedulerFactory(new NameValueCollection { { "quartz.serializer.type", "binary" } });
            var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);

            await scheduler.Start().ConfigureAwait(false);

            //await ScheduleJob<RefreshExchangeRatesJob>(scheduler, TimeSpan.FromMinutes(1));
            //await ScheduleJob<RefreshTransactionsJob>(scheduler, TimeSpan.FromHours(1));
            await ScheduleJob<RefreshTokenBalanceJob>(scheduler, TimeSpan.FromHours(1));
            //await ScheduleJob<TransferCryptoAssetsJob>(scheduler, TimeSpan.FromDays(1));

            WriteLine("Press the escape key (ESC) to quit.");
            while (ReadKey(true).Key != ConsoleKey.Escape)
            {
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }

            await scheduler.Shutdown().ConfigureAwait(false);
        }

        private static async Task ScheduleJob<TJob>(IScheduler scheduler, TimeSpan interval)
        {
            var job = JobBuilder
                .Create<RefreshTransactionsJob>()
                .WithIdentity(typeof(TJob).Name)
                .Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity($"{typeof(TJob).Name}Trigger")
                .StartNow()
                .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
