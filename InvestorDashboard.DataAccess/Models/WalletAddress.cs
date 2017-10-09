using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.DataAccess.Models
{
    public class WalletAddress
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public string Currency { get; set; }
        public string Address { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
