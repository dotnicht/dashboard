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
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        [Required]
        public string TransactionIndex { get; set; }
        [Required]
        public string TransactionHash { get; set; }
    }
}
