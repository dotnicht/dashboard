﻿using AutoMapper;
using InvestorDashboard.Backend.Database.Models;
using InvestorDashboard.Backend.Models;
using InvestorDashboard.Backend.Services.Implementation;
using System;

namespace InvestorDashboard.Backend
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EthereumService.EtherscanResponse.Transaction, CryptoTransaction>()
                .ForMember(x => x.Sender, x => x.MapFrom(y => y.From))
                .ForMember(x => x.Recipient, x => x.MapFrom(y => y.To))
                .ForMember(x => x.Amount, x => x.MapFrom(y => double.Parse(y.Value)/Math.Pow(10, 18)))
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => DateTimeOffset.FromUnixTimeSeconds(long.Parse(y.TimeStamp)).UtcDateTime));
            CreateMap<EthereumService.EtherchainResponse.Transaction, CryptoTransaction>()
                .ForMember(x => x.TimeStamp, x => x.MapFrom(y => y.Time));
            CreateMap<CryptoTransaction, Transaction>()
                .ForMember(x => x.CounterpartyAddress, x => x.MapFrom(y => y.Sender));
        }
    }
}
