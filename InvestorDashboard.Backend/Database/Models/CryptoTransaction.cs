using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class CryptoTransaction
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Guid CryptoAddressId { get; set; }
        [ForeignKey("CryptoAddressId")]
        public CryptoAddress CryptoAddress { get; set; }
        public CryptoTransactionDirection Direction { get; set; }
        public string Hash { get; set; }
        public string Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? ExternalId { get; set; }
        public bool IsNotified { get; set; }
        public bool? IsFailed { get; set; }
        public bool IsReferralPaid { get; set; }
        public bool IsSpent { get; set; }
        public bool IsInactive { get; set; }
    }
}
