using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public abstract class JobBase : IJob, IDisposable
    {
        private bool isDisposed;

        public abstract TimeSpan Period { get; }
        protected ApplicationDbContext Context { get; }
        protected IOptions<JobsSettings> Options { get; }
        protected ILogger Logger { get; }

        protected JobBase(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options)
        {
            Logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(GetType());
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                await ExecuteInternal(context);
            }
            catch (Exception ex)
            {
                //await Out.WriteLineAsync($"An error occurred while executing job {GetType().Name}");
                //await Out.WriteLineAsync(ex.ToString());

                Logger.LogError(ex, $"An error occurred while executing the job.");
            }
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

        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}
