using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.ConfigurationSections
{
    public class JobsSettings
    {
        public int ThreadCount { get; set; }
        public Dictionary<string, JobSettings> Jobs { get; set; }

        public class JobSettings
        {
            public TimeSpan Period { get; set; }
            public bool StartImmediately { get; set; }
            public bool IsDisabled { get; set; }
            public bool IsInfinite { get; set; }
        }
    }
}
