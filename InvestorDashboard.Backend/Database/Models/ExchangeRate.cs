using InvestorDashboard.Backend.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ExchangeRate
    {
        public Guid Id { get; set; }
        public Currency Base { get; set; } 
        public Currency Quote { get; set; }
        public decimal Rate { get; set; }
        public DateTime Created { get; set; }
    }
}
