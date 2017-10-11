using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Backend.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public decimal Balance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string KeyStore { get; set; }
    }
}
