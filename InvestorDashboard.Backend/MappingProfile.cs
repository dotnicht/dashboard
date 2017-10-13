using AutoMapper;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services.Implementation;

namespace InvestorDashboard.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EthereumService.EtherscanResponse.EtherscanTransaction, EthereumTransaction>();
        }
    }
}
