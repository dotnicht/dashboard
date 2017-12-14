using InvestorDashboard.Backend.Database.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ExchangeRate
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public Currency Base { get; set; } 
        public Currency Quote { get; set; }
        public decimal Rate { get; set; }
    }
}
