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
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal TokenPrice { get; set; }
        public decimal BonusPercentage { get; set; }
        public Guid? ExternalId { get; set; }
        public bool IsNotified { get; set; }
    }
}
