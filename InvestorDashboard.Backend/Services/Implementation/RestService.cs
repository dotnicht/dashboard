using System;
using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;

namespace InvestorDashboard.Backend.Services.Implementation
{
    internal class RestService : IRestService
    {
        public TResponse Get<TResponse>(Uri uri) where TResponse : class, new()
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var client = new RestClient(uri);
            var request = new RestRequest(Method.GET);
            var response = client.Execute<TResponse>(request).Result;

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Remote server returned status {response.StatusCode}. URI: {uri}.");
            }

            if (response == null || response.Data == null)
            {
                throw new InvalidOperationException($"Remote server returned empty data response. URI: {uri}.");
            }

            return response.Data;
        }

        public async Task<TResponse> GetAsync<TResponse>(Uri uri) where TResponse : class, new()
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var client = new RestClient(uri);
            var request = new RestRequest(Method.GET);
            var response = await client.Execute<TResponse>(request);
            
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Remote server returned status {response.StatusCode}. URI: {uri}.");
            }

            if (response == null || response.Data == null)
            {
                throw new InvalidOperationException($"Remote server returned empty data response. URI: {uri}.");
            }

            return response.Data;
        }
    }
}
