using InvestorDashboard.Backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class WalletAddress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        public Currency Currency { get; set; }
        public string Address { get; set; }
        public DateTime Created { get; set; }
    }
}
