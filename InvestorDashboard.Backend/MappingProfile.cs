using AutoMapper;
using Info.Blockchain.API.Models;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services.Implementation;
using System;

namespace InvestorDashboard.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EthereumService.EtherscanAccountResponse.Transaction, CryptoTransaction>()
                .ForMember(x => x.Amount, x => x.MapFrom(y => y.Value))
                .ForMember(x => x.Timestamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(long.Parse(y.TimeStamp)).UtcDateTime));

            CreateMap<Transaction, CryptoTransaction>()
                .ForMember(x => x.Timestamp, x => x.MapFrom(y => y.Time));

            CreateMap<ApplicationUser, UserProfile>()
                .ForMember(x => x.Id, x => x.Ignore());
        }
    }
}
