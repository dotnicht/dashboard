using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
