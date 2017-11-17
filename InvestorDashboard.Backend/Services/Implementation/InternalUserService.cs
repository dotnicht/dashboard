using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Database;
using Microsoft.Extensions.Logging;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class InternalUserService : ContextService, IInternalUserService
    {
        public InternalUserService(ApplicationDbContext context, ILoggerFactory loggerFactory) 
            : base(context, loggerFactory)
        {
        }

        public Task<decimal> GetInternalUserBalance(string userId)
        {
            throw new NotImplementedException();
        }

        public Task SyncInternalUsers()
        {
            throw new NotImplementedException();
        }
    }
}
