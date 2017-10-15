using RestSharp.Portable;
using RestSharp.Portable.HttpClient;

namespace InvestorDashboard.Backend
{
    public static class RestUtil
    {
        public static T Get<T>(string url) where T : new()
        {
            var client = new RestClient(url);
            var request = new RestRequest(Method.GET);
            var task = client.Execute<T>(request);
            return task.Result.Data;
        }
    }
}
