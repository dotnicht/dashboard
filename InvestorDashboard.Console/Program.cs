using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using Hangfire;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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
            var serviceCollection = new ServiceCollection()
              .AddAutoMapper(typeof(Configuration));

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", false, true)
                .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            Configuration.Configure(serviceCollection, configuration);

            serviceCollection.AddSingleton(new LoggerFactory().Initialize());

            ApplicationDbContext.Initialize(serviceCollection, configuration);


            var serviceProvider = serviceCollection
                .BuildServiceProvider();

            var settings = serviceProvider
                .GetRequiredService<IOptions<JobsSettings>>()
                .Value;

            var propr = new NameValueCollection
            {
                { $"{StdSchedulerFactory.PropertyObjectSerializer}.type", "binary" },
                { $"{StdSchedulerFactory.PropertyThreadPoolPrefix}.threadCount", settings.ThreadCount.ToString() }
            };

            var schedulerFactory = new StdSchedulerFactory(propr);
            var scheduler = await schedulerFactory.GetScheduler().ConfigureAwait(false);

            Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(x => !x.IsAbstract && typeof(IJob).IsAssignableFrom(x))
                .ToList()
                .ForEach(async x =>
                {
                    if (settings.Jobs.ContainsKey(x.Name) && !settings.Jobs[x.Name].IsDisabled)
                    {
                        serviceCollection.AddTransient(x);

                        var job = JobBuilder
                            .Create(x)
                            .Build();

                        var builder = TriggerBuilder.Create();

                        if (settings.Jobs[x.Name].StartImmediately)
                        {
                            builder = builder.StartNow();
                        }

                        if (settings.Jobs[x.Name].Period != default(TimeSpan))
                        {
                            builder = builder.WithSimpleSchedule(y => y.WithInterval(settings.Jobs[x.Name].Period).RepeatForever());
                        }

                        await scheduler.ScheduleJob(job, builder.Build());

                        serviceProvider
                            .GetRequiredService<ILogger<JobFactory>>()
                            .LogInformation($"Job {x} execution scheduled.");
                    }
                });

            scheduler.JobFactory = new JobFactory(serviceCollection.BuildServiceProvider());

            await scheduler.Start().ConfigureAwait(false);

            while (System.Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            await scheduler.Shutdown().ConfigureAwait(false);
        }
    }
}
