using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class EthereumBlock
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public long BlockIndex { get; set; }
        [Required]
        public string BlockHash { get; set; }
        public virtual ICollection<EthereumTransaction> Transactions { get; set; }
    }
}
