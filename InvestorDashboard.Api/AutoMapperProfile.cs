using AutoMapper;
using InvestorDashboard.Api.Models;
using InvestorDashboard.Api.Models.AccountViewModels;
using InvestorDashboard.Api.Models.DashboardModels;
using InvestorDashboard.Backend.ConfigurationSections;
using InvestorDashboard.Backend.Database.Models;
using Microsoft.AspNetCore.Identity;

namespace InvestorDashboard.Api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterViewModel, ApplicationUser>()
                .ForMember(x => x.RegistrationUri, x => x.MapFrom(y => y.StartUrl));

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

            CreateMap<DashboardHistoryItem, IcoInfoModel>();

            CreateMap<TokenSettings, IcoInfoModel>()
                .ForMember(x => x.TokenPrice, x => x.MapFrom(y => y.Price))
                .ForMember(x => x.TokenName, x => x.MapFrom(y => y.Name));

            CreateMap<ApplicationUser, ClientInfoModel>();
        }
    }
}
