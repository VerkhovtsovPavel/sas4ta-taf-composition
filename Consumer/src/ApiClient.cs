using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Consumer
{
    public class ApiClient
    {
        private readonly Uri BaseUri;

        public ApiClient(Uri baseUri)
        {
            this.BaseUri = baseUri;
        }
        public async Task<HttpResponseMessage> GetAllProducts()
        {
            return await SendRequest($"/api/products");
        }

        public async Task<HttpResponseMessage> GetProduct(int id)
        {
            return await SendRequest($"/api/products/{id}");
        }

        private async Task<HttpResponseMessage> SendRequest(string url)
        {
            using (var client = new HttpClient { BaseAddress = BaseUri })
            {
                try
                {
                    client.DefaultRequestHeaders.Add("Authorization", AuthorizationHeaderValue());
                    return await client.GetAsync(url);
                }
                catch (Exception ex)
                {
                    throw new Exception("There was a problem connecting to Provider API.", ex);
                }
            }
        }

        private string AuthorizationHeaderValue()
        {
            return $"Bearer {DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}";
        }
    }
}
