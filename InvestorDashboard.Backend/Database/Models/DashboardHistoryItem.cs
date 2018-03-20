using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class DashboardHistoryItem
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public int TotalUsers { get; set; }
        public int TotalInvestors { get; set; }
        public string TotalInvested { get; set; }
        public int TotalNonInternalUsers { get; set; }
        public int TotalNonInternalInvestors { get; set; }
        public string TotalNonInternalInvested { get; set; }
        public Currency Currency { get; set; }
    }
}
