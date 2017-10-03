using Microsoft.AspNetCore.Identity;
using System;

namespace InvestorDashboard.DataAccess.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public bool IsEnabled { get; set; }
        public bool IsLockedOut => LockoutEnabled && LockoutEnd >= DateTimeOffset.UtcNow;
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
    }
}
