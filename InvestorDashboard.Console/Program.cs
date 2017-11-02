using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Services;
using InvestorDashboard.Console.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

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
                .AddLogging(x => x.AddConsole())
                .AddAutoMapper(typeof(DependencyInjection));

            Configuration.Configure(serviceCollection, configuration);
            DependencyInjection.Configure(serviceCollection);

            var keyVaultService = serviceCollection
                .BuildServiceProvider()
                .GetRequiredService<IKeyVaultService>();

            serviceCollection.AddDbContext<ApplicationDbContext>(
                x => x.UseSqlServer(keyVaultService.DatabaseConnectionString, y => y.MigrationsAssembly("InvestorDashboard.Backend")),
                ServiceLifetime.Transient);

            var schedulerFactory = new StdSchedulerFactory(new NameValueCollection { { "quartz.serializer.type", "binary" } });
            var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);

            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => !x.IsAbstract && typeof(IJob).IsAssignableFrom(x))
                .ToList()
                .ForEach(async x =>
                {
                    serviceCollection.AddTransient(x);

                    var job = JobBuilder
                        .Create(x)
                        .Build();

                    var period = (serviceCollection.BuildServiceProvider().GetService(x) as JobBase)?.Period;

                    if (period != null)
                    {
                        var trigger = TriggerBuilder
                            .Create()
                            .StartNow()
                            .WithSimpleSchedule(y => y.WithInterval(period.Value).RepeatForever())
                            .Build();

                        await scheduler.ScheduleJob(job, trigger);
                    }
                });

            scheduler.JobFactory = new JobFactory(serviceCollection.BuildServiceProvider());

            await scheduler.Start().ConfigureAwait(false);

            while (ReadKey(true).Key != ConsoleKey.Escape)
            {
                Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            }

            await scheduler.Shutdown().ConfigureAwait(false);
        }
    }
}
