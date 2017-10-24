using System;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Options;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public abstract class JobBase : IJob
    {
        public abstract TimeSpan Period { get; }
        protected ApplicationDbContext Context { get; }
        protected IOptions<JobsSettings> Options { get; }

        public JobBase(ApplicationDbContext context, IOptions<JobsSettings> options)
        {
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
                await Out.WriteLineAsync($"An error occurred while executing job {GetType().Name}");
                await Out.WriteLineAsync(ex.ToString());
            }
        }

        protected abstract Task ExecuteInternal(IJobExecutionContext context);
    }
}
