using InvestorDashboard.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace InvestorDashboard.Api.Extensions
{
  public static class UrlHelperExtensions
  {
    //public static string EmailConfirmationLink(this IUrlHelper urlHelper, Guid userId, string code, string scheme)
    //{
    //  return urlHelper.Action(
    //    action: nameof(AccountController.ConfirmEmail),
    //    controller: "Account",
    //    values: new { userId, code },
    //    protocol: scheme);
    //}

    public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, String email, string code, string scheme)
    {
      return urlHelper.Action(
        action: nameof(AccountController.ResetPassword),
        controller: "Account",
        values: new { email, code },
        protocol: scheme);
    }
  }
}
