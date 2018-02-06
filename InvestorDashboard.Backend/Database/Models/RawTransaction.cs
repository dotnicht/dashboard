using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class RawTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string TransactionHash { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public long BlockIndex { get; set; }
        public Currency Currency { get; set; }
    }
}
