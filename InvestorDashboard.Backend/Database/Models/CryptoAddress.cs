using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class CryptoAddress : IEquatable<CryptoAddress>
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
        public long StartBlockIndex { get; set; }
        public long? LastBlockIndex { get; set; }
        public DateTime? LastUpdated { get; set; }
        public virtual ICollection<CryptoTransaction> CryptoTransactions { get; set; }

        public bool Equals(CryptoAddress other)
        {
            return other != null 
                && other.Address == Address
                && other.UserId == UserId 
                && other.IsDisabled == IsDisabled 
                && other.Type == Type 
                && other.Currency == Currency;
        }
    }
}
