using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace InvestorDashboard.Backend
{
    public static class Logging
    {
        public static ILoggerFactory Initialize(this ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            var name = Assembly.GetEntryAssembly().GetName().Name;

            return loggerFactory
                .AddConsole(LogLevel.Warning)
                .AddFile($"Logs/{name}-Full.{{Date}}.log")
                .AddFile($"Logs/{name}-Information.{{Date}}.log", levelOverrides: new Dictionary<string, LogLevel> { { DbLoggerCategory.Name, LogLevel.Warning } })
                .AddFile($"Logs/{name}-Warning.{{Date}}.log", LogLevel.Warning);
        }
    }
}
