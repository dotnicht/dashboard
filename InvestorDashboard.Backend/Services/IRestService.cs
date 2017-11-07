using System;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend.Services
{
    public interface IRestService<TResponse> where TResponse : class, new()
    {
        TResponse Get(Uri uri);
        Task<TResponse> GetAsync(Uri uri);
    }
}
