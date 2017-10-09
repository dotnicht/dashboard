using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.DataAccess.Models
{
    public class Payment
    {
        [Key]
        public Guid Id { get; set; }
        public Guid WalletAddressId { get; set; }
        [ForeignKey("WalletAddressId")]
        public WalletAddress WalletAddress { get; set; }
        public string UserId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
