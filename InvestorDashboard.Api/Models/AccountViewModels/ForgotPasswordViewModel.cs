using System.ComponentModel.DataAnnotations;
using InvestorDashboard.Api.Models;

namespace InvestorDashboard.Api.Models.AccountViewModels
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
    }
    public class ResendEmailConfirmCodeViewModel
    {
        public string Email { get; set; }
    }
}
