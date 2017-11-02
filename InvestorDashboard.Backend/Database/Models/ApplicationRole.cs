using Microsoft.AspNetCore.Identity;

namespace InvestorDashboard.Backend.Database.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }

        public ApplicationRole()
        {
        }

        public ApplicationRole(string roleName)
            : base(roleName)
        {
        }

        public ApplicationRole(string roleName, string description)
            : base(roleName)
        {
            Description = description;
        }
    }
}
