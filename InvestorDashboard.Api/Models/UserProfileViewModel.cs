using System.ComponentModel.DataAnnotations;

namespace InvestorDashboard.Api.Models
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Photo { get; set; }
        public string TelegramUsername { get; set; }
    }
}
