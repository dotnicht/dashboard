using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ApplicationUser : IdentityUser
    {
        public long Balance { get; set; }
        public long BonusBalance { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string Configuration { get; set; }
        public string City { get; set; }
        public bool IsTokenSaleDisabled { get; set; }
        public bool IsEligibleForTransfer { get; set; }
        public string PhoneCode { get; set; }
        public string ClickId { get; set; }
        public Guid? ExternalId { get; set; }
        public string ReferralCode { get; set; }
        public string ReferralUserId { get; set; }
        [ForeignKey("ReferralUserId")]
        public ApplicationUser ReferralUser { get; set; }
        public ICollection<ApplicationUser> Referrals { get; set; }
        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
        public virtual ICollection<CryptoAddress> CryptoAddresses { get; set; }
    }
}