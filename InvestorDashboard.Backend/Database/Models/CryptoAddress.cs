﻿using InvestorDashboard.Backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class CryptoAddress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CryptoAccountId { get; set; }
        [ForeignKey("CryptoAccountId")]
        public CryptoAccount CryptoAccount { get; set; }
        public CryptoAddressType Type { get; set; }
        public string Address { get; set; }
        public DateTime Created { get; set; }
    }
}
