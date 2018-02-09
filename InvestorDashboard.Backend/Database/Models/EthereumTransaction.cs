using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class EthereumTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Guid BlockId { get; set; }
        [ForeignKey("BlockId")]
        public EthereumBlock Block { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        public long TransactionIndex { get; set; }
        [Required]
        public string TransactionHash { get; set; }
        public decimal Amount { get; set; }
    }
}
