using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public int Balance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public bool IsEligibleForTokenSale { get; set; }
    }
}
