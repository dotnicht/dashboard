using InvestorDashboard.Business.ConfigurationSections;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestorDashboard.Business.Implementation
{
    internal class BitGoService : IBitGoService
    {
        private readonly IOptions<BitGo> _options;

        public BitGoService(IOptions<BitGo> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
