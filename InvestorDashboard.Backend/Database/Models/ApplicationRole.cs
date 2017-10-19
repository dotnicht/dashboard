using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace InvestorDashboard.Backend.Database.Models
{
  public class ApplicationRole : IdentityRole
  {
    public ApplicationRole()
    {

    }

    public ApplicationRole(string roleName) : base(roleName)
    {

    }

    public ApplicationRole(string roleName, string description) : base(roleName)
    {
      Description = description;
    }

    public string Description { get; set; }

  }
}
