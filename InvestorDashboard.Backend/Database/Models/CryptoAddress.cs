using InvestorDashboard.Backend.Database.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Database.Models
{
    public class CryptoAddress
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId"), Required]
        public ApplicationUser User { get; set; }
        public Currency Currency { get; set; }
        public CryptoAddressType Type { get; set; }
        public string PrivateKey { get; set; }
        public string Address { get; set; }
        public bool IsDisabled { get; set; }
        public virtual ICollection<CryptoTransaction> CryptoTransactions { get; set; }
    }
}
