using InvestorDashboard.Backend.ConfigurationSections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InvestorDashboard.Console.Jobs
{
    public abstract class JobBase : IJob
    {
        protected IOptions<JobsSettings> Options { get; }
        protected ILogger Logger { get; }

        protected JobBase(ILoggerFactory loggerFactory, IOptions<JobsSettings> options)
        {
            Logger = loggerFactory?.CreateLogger(GetType()) ?? throw new ArgumentNullException(nameof(loggerFactory));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var sw = Stopwatch.StartNew();
            Logger.LogInformation($"Job {GetType()} execution started.");

            try
            {
                await ExecuteInternal(context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An error occurred while executing the job {GetType()}.");
            }

            Logger.LogInformation($"Job {GetType()} execution elapsed {sw.Elapsed}.");

            if (Options.Value.Jobs[GetType().Name].Period < sw.Elapsed)
            {
                Logger.LogWarning($"Job {GetType()} execution elapsed {sw.Elapsed} which is more then estimated {Options.Value.Jobs[GetType().Name].Period}.");
            }
        }

        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}
