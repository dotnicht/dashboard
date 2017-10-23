using InvestorDashboard.Backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Database.Models
{
    public class CryptoAccount
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId"), Required]
        public ApplicationUser User { get; set; }
        public Currency Currency { get; set; }
        [Required]
        public string KeyStore { get; set; }
        public DateTime Created { get; set; }
        public bool IsDisabled { get; set; }
        public virtual ICollection<CryptoAddress> CryptoAddresses { get; set; }
    }
}
