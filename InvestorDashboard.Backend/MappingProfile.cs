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
            CreateMap<EthereumService.AccountTransactionsResponseResult, CryptoTransaction>()
                .ForMember(x => x.Amount, x => x.MapFrom(y => double.Parse(y.Value)/Math.Pow(10, 18)))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(long.Parse(y.TimeStamp)).UtcDateTime));

            CreateMap<EthereumService.AccountInternalTransactionsResponseResult, CryptoTransaction>()
                .ForMember(x => x.Amount, x => x.MapFrom(y => double.Parse(y.Value) / Math.Pow(10, 18)))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(long.Parse(y.TimeStamp)).UtcDateTime));

            CreateMap<BitcoinService.ChainResponse.Transaction, CryptoTransaction>()
                .ForMember(x => x.Hash, x => x.MapFrom(y => y.Txid))
                .ForMember(x => x.Amount, x => x.MapFrom(y => decimal.Parse(y.Incoming.Value)))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(y.Time).UtcDateTime));

            CreateMap<BitcoinService.BlockExplorerResponse.Tx, CryptoTransaction>()
                .ForMember(x => x.Hash, x => x.MapFrom(y => y.Txid))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(y.Time).UtcDateTime));

            CreateMap<Transaction, CryptoTransaction>()
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => y.Time));
        }
    }
}
