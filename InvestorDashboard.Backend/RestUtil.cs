using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using System.Threading.Tasks;
using System.Net;
using System;

namespace InvestorDashboard.Backend
{
    public static class RestUtil
    {
        public static async Task<T> Get<T>(string url) where T : class, new()
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var response = await client.Execute<T>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Remote server returned status {response.StatusCode}. URI: {url}.");
            }

            return response?.Data;
        }
    }
}
