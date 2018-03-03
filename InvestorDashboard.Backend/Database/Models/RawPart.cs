using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class RawPart
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Guid TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public RawTransaction Transaction { get; set; }
        public RawPartType Type { get; set; }
        public string Reference { get; set; }
        [Required]
        public string Index { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
