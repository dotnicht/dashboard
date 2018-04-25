using System;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class ContextService
    {
        private bool isDisposed;

        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Logger { get; }

        protected ContextService(ApplicationDbContext context, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Logger = loggerFactory?.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected ApplicationDbContext CreateContext()
        {
            return ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }
    }
}
