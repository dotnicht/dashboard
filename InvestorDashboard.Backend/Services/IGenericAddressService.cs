﻿using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IGenericAddressService : IDisposable
    {
        Task CreateMissingAddresses();
    }
}