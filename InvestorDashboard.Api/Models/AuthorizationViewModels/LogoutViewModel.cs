using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InvestorDashboard.Api.Models.AuthorizationViewModels
{
    public class LogoutViewModel
    {
      [BindNever]
      public string RequestId { get; set; }
  }
}
