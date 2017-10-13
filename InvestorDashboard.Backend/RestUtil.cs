using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System.Threading.Tasks;

namespace InvestorDashboard.Backend
{
    public static class RestUtil
    {
        public static async Task<T> Get<T>(string url) where T : new()
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var response = await client.Execute<T>(request).ConfigureAwait(false);
            return response.Data;
        }
    }
}
