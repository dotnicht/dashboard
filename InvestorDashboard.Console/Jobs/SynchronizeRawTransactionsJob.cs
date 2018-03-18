using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace InvestorDashboard.Console.Jobs
{
    public class SynchronizeRawTransactionsJob : JobBase
    {
        private readonly IEnumerable<ICryptoService> _cryptoServices;

        public SynchronizeRawTransactionsJob(ILoggerFactory loggerFactory, ApplicationDbContext context, IOptions<JobsSettings> options, IEnumerable<ICryptoService> cryptoServices)
            : base(loggerFactory, context, options)
        {
            _cryptoServices = cryptoServices ?? throw new ArgumentNullException(nameof(cryptoServices));
        }

        protected override async Task ExecuteInternal(IJobExecutionContext context)
        {
            while (true)
            {
                using (var cts = new CancellationTokenSource())
                {
                    try
                    {
                        Task.WaitAll(_cryptoServices.Select(x => x.SynchronizeRawTransactions(cts.Token)).ToArray());
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"An error occurrred while executing job {GetType()}.");
                        cts.Cancel();
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            _cryptoServices.ToList().ForEach(x => x.Dispose());
            base.Dispose(disposing);
        }
    }
}
