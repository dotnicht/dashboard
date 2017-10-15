﻿using InvestorDashboard.Backend.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; }
        public Guid WalletAddressId { get; set; }
        [ForeignKey("WalletAddressId")]
        public CryptoAddress Address { get; set; }
        public string CounterpartyAddress { get; set; }
        public string Hash { get; set; }
        public TransactionDirection Direction { get; set; }
        public Currency Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal ExchangeRate { get; set; }
        public decimal TokenPrice { get; set; }
        public DateTime Created { get; set; }
    }
}
