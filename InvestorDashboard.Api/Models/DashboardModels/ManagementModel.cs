using System;

namespace InvestorDashboard.Api.Models.DashboardModels
{
    public class ManagementModel
    {
        public Guid Id { get; set; }
        public ManagementTransaction[] Transactions { get; set; }
            
        public class ManagementTransaction
        {
            public Guid Id { get; set; }
            public DateTime Timestamp { get; set; }
            public long Amount { get; set; }
            public bool IsInactive { get; set; }
        }
    }
}
