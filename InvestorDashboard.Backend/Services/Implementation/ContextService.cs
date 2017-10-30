using System;
using InvestorDashboard.Backend.Database;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal abstract class ContextService : IDisposable
    {
        private bool isDisposed;
        protected ApplicationDbContext Context { get; }

        public ContextService(ApplicationDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
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
