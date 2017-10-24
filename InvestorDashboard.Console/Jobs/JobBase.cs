using System;
using System.Threading.Tasks;
using Quartz;
using static System.Console;

namespace InvestorDashboard.Console.Jobs
{
    public abstract class JobBase : IJob
    {
        public abstract TimeSpan Period { get; }

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
