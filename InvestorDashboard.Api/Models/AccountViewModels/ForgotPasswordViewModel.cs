using System.ComponentModel.DataAnnotations;
using InvestorDashboard.Api.Models;

namespace InvestorDashboard.Api.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
  {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
