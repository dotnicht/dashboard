using System;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class ContextService : IDisposable
    {
        private bool isDisposed;

        protected ApplicationDbContext Context { get; }
        protected ILogger Logger { get; }

        protected ContextService(ApplicationDbContext context, ILoggerFactory loggerFactory)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = loggerFactory?.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }

                isDisposed = true;
            }
        }
    }
}
