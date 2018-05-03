using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static InvestorDashboard.Backend.ConfigurationSections.TokenSettings.BonusSettings;

namespace InvestorDashboard.Api.Models
{
    public class UserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Username is required"),
         StringLength(200, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 200 characters")]
        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required"),
         StringLength(200, ErrorMessage = "Email must be at most 200 characters"),
         EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }

        public string City { get; set; }

        [MaxLength(3)]
        public string CountryCode { get; set; }

        public string[] Roles { get; set; }
        public string Photo { get; set; }
        public string TelegramUsername { get; set; }

        public Dictionary<BonusCriterion, (bool Status, long Amount)> KycStatus { get; set; }
    }
}
