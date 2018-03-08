using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class RawTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Guid BlockId { get; set; }
        [ForeignKey("BlockId")]
        public RawBlock Block { get; set; }
        [Required]
        public string Hash { get; set; }
        public ICollection<RawPart> Parts { get; set; }
    }
}
