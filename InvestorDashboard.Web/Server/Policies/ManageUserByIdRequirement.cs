using InvestorDashboard.Backend.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Web.Server.Helpers;

namespace InvestorDashboard.Web.Server.Policies
{
    public class ManageUserByIdRequirement : IAuthorizationRequirement
    {

    }


    public class ManageUserByIdHandler : AuthorizationHandler<ManageUserByIdRequirement, string>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ManageUserByIdRequirement requirement, string userId)
        {
            if (context.User.HasClaim(CustomClaimTypes.Permission, ApplicationPermissions.ManageUsers) || GetIsSameUser(context.User, userId))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }


        private bool GetIsSameUser(ClaimsPrincipal user, string userId)
        {
            return Utilities.GetUserId(user) == userId;
        }
    }
}
