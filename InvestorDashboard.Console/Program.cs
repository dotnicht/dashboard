﻿using AspNet.Security.OpenIdConnect.Primitives;
using AutoMapper;
using InvestorDashboard.Backend;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Console.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
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
                .AddAutoMapper(typeof(DependencyInjection));

            var configurationBuilder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", false, true)
              .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
              .AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            Configuration.Configure(serviceCollection, configuration);
            DependencyInjection.Configure(serviceCollection);

            SetupLogging(serviceCollection);

            SetupIdentity(serviceCollection);

            serviceCollection.AddDbContext<ApplicationDbContext>(
                x => x.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), y => y.MigrationsAssembly("InvestorDashboard.Backend")),
                ServiceLifetime.Transient);

            await SetupScheduling(serviceCollection);
        }

        private static void SetupIdentity(IServiceCollection serviceCollection)
        {
            serviceCollection.AddIdentity<ApplicationUser, ApplicationRole>(config => config.SignIn.RequireConfirmedEmail = true)
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();

            serviceCollection.Configure<IdentityOptions>(options =>
            {
                // User settings
                options.User.RequireUniqueEmail = true;

                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                options.Lockout.MaxFailedAccessAttempts = 10;

                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
            });
        }

        private static void SetupLogging(IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddNLog();
            });
        }

        private static async Task SetupScheduling(IServiceCollection serviceCollection)
        {
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

                    if (period != null && period.Value != default(TimeSpan))
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

            while (System.Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            await scheduler.Shutdown().ConfigureAwait(false);
        }
    }
}
