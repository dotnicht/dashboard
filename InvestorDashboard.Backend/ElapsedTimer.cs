using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace InvestorDashboard.Backend
{
    public sealed class ElapsedTimer : IDisposable
    {
        private readonly ILogger _logger;
        private readonly string _name;
        private readonly Stopwatch _stopwatch;

        public ElapsedTimer(ILogger logger, string name)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _logger.LogInformation($"Execution {_name} elapsed {_stopwatch.Elapsed}");
        }
    }
}
