
using AutoMapper;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvestorDashboard.Backend.Core;
using InvestorDashboard.Backend.Database.Models;
using StackExchange.Redis;

namespace InvestorDashboard.Web.Server.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>()
                   .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserViewModel, ApplicationUser>()
                .ForMember(d => d.Roles, map => map.Ignore());

            CreateMap<ApplicationUser, UserEditViewModel>()
                .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserEditViewModel, ApplicationUser>()
                .ForMember(d => d.Roles, map => map.Ignore());

            CreateMap<ApplicationUser, UserPatchViewModel>()
                .ReverseMap();

            CreateMap<ApplicationRole, RoleViewModel>()
                .ForMember(d => d.Permissions, map => map.MapFrom(s => s.Claims))
                .ForMember(d => d.UsersCount, map => map.ResolveUsing(s => s.Users?.Count ?? 0))
                .ReverseMap()
                .ForMember(d => d.Claims, map => map.Ignore());

            CreateMap<IdentityRoleClaim<string>, ClaimViewModel>()
                .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
                .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
                .ReverseMap();

            CreateMap<ApplicationPermission, PermissionViewModel>()
                .ReverseMap();

            CreateMap<IdentityRoleClaim<string>, PermissionViewModel>()
                .ConvertUsing(s => Mapper.Map<PermissionViewModel>(ApplicationPermissions.GetPermissionByValue(s.ClaimValue)));
        }
    }
}
