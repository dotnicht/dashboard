using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ApplicationUser : IdentityUser
    {
        public decimal Balance { get; set; }
        public decimal BonusBalance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string Configuration { get; set; }
        public string City { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public string PhoneCode{ get; set; }
        public Guid? ExternalId { get; set; }
        public string ClickId { get; set; }
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
        public virtual ICollection<CryptoAddress> CryptoAddresses { get; set; }
    }
}
