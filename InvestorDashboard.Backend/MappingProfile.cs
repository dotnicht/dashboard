using AutoMapper;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services.Implementation;

namespace InvestorDashboard.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EthereumService.EtherscanResponse.EtherscanTransaction, EthereumTransaction>();
            CreateMap<EthereumTransaction, Transaction>()
                .ForMember(x => x.CounterpartyAddress, x => x.MapFrom(y => y.Sender));
        }
    }
}
