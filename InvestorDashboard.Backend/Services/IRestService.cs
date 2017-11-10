using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IRestService
    {
        TResponse Get<TResponse>(Uri uri) where TResponse : class, new();
        Task<TResponse> GetAsync<TResponse>(Uri uri) where TResponse : class, new();
    }
}
