using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InvestorDashboard.Web.Server.Models.AuthorizationViewModels
{
    public class AuthorizeViewModel
    {
      [Display(Name = "Application")]
      public string ApplicationName { get; set; }

      [BindNever]
      public string RequestId { get; set; }

      [Display(Name = "Scope")]
      public string Scope { get; set; }
  }
}
