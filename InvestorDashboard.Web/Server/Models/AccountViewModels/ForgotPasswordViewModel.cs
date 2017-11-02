using System.ComponentModel.DataAnnotations;
using InvestorDashboard.Web.Server.Models;

namespace InvestorDashboard.Web.Models.AccountViewModels
{
    public class ForgotPasswordViewModel : UserViewModel
  {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
