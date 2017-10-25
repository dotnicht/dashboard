using AutoMapper;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Services.Implementation;
using System;

namespace InvestorDashboard.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CryptoAddress, CryptoAddress>()
                .ForMember(x => x.Id, x => x.UseValue(Guid.Empty));

            CreateMap<EthereumService.EtherscanResponse.Transaction, CryptoTransaction>()
                .ForMember(x => x.Amount, x => x.MapFrom(y => double.Parse(y.Value)/Math.Pow(10, 18)))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(long.Parse(y.TimeStamp)).UtcDateTime));
        }
    }
}
