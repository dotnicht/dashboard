﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ConfigurationItem
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}