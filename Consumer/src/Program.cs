using System;

namespace Consumer
{
    class Program
    {
         private static readonly string ProviderUri = "http://localhost:9001";

        static void Main(string[] args)
        {
            var ApiClient = new ApiClient(new Uri(ProviderUri));

            Console.WriteLine("**Retrieving product list**");
            var response = ApiClient.GetAllProducts().GetAwaiter().GetResult();
            var responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Response.Code={response.StatusCode}, Response.Body={responseBody}\n\n");

            int productId = 10;
            Console.WriteLine($"**Retrieving product with id={productId}");
            response = ApiClient.GetProduct(productId).GetAwaiter().GetResult();
            responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            Console.WriteLine($"Response.Code={response.StatusCode}, Response.Body={responseBody}");
        }
    }
}
