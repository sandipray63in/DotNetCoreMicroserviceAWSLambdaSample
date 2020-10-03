using Domain.Base.Entities;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class HttpClientFactoryExtension
    {
        public static async Task<TEntity> GetDataAsync<TEntity, TId>(this IHttpClientFactory httpClientFactory, string url, string accessToken = null, string httpClientName = null)
            where TEntity : BaseEntity<TId>
            where TId : struct
        {
            var httpClient = httpClientName.IsNotNullOrEmpty() ? httpClientFactory.CreateClient(httpClientName) : httpClientFactory.CreateClient();
            using (httpClient)
            {
                if (accessToken.IsNotNullOrEmpty())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
                }
                var dataString = await httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<TEntity>(dataString);
            }
        }
    }
}
