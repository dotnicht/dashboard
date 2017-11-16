using AutoMapper;
using InvestorDashboard.Api.Models.AccountViewModels;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;

namespace InvestorDashboard.Api.Models
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterViewModel, ApplicationUser>();
            CreateMap<ApplicationUser, UserViewModel>()
                   .ForMember(d => d.Roles, map => map.Ignore());
            CreateMap<UserViewModel, ApplicationUser>();

            CreateMap<ApplicationUser, UserEditViewModel>();
            CreateMap<UserEditViewModel, ApplicationUser>();

            CreateMap<ApplicationUser, UserPatchViewModel>()
                .ReverseMap();

            CreateMap<IdentityRoleClaim<string>, ClaimViewModel>()
                .ForMember(d => d.Type, map => map.MapFrom(s => s.ClaimType))
                .ForMember(d => d.Value, map => map.MapFrom(s => s.ClaimValue))
                .ReverseMap();


        }
    }
}
