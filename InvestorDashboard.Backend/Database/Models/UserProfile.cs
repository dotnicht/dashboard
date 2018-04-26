using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestorDashboard.Backend.Database.Models
{
    public class UserProfile
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId"), Required]
        public ApplicationUser User { get; set; }
        public bool IsDisabled { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [MaxLength(3)]
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string Photo { get; set; }
        public string PhoneCode { get; set; }
        public string TelegramUsername { get; set; }
    }
}
