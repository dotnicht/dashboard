using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using InvestorDashboard.Web.Server.Models;

namespace InvestorDashboard.Web.Server.Models.AccountViewModels
{
  public class RegisterViewModel : UserViewModel
  {
    public string Password { get; set; }
  }
}
