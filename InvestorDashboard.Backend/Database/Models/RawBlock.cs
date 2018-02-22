using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class RawBlock
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Timestamp { get; set; }
        [Required]
        public string BlockIndex { get; set; }
        [Required]
        public string BlockHash { get; set; }
        public Currency Currency { get; set; }
        public virtual ICollection<RawTransaction> Transactions { get; set; }
    }
}
