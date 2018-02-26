using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IRestService
    {
        Task<TResponse> GetAsync<TResponse>(Uri uri) 
            where TResponse : class, new();
    }
}
