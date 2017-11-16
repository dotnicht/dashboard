using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Api.Models;

namespace InvestorDashboard.Api.Models.AccountViewModels
{
  public class RegisterViewModel : UserViewModel
  {
        public string RegistrationRequest { get; set; }
        public string Password { get; set; }
  }
}
