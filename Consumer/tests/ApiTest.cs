using System.IO;
using PactNet;
using PactNet.Native;
using Xunit.Abstractions;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Consumer;
using PactNet.Matchers;
using Allure.Commons;
using Allure.Xunit;
using Allure.Xunit.Attributes;

namespace tests
{
    [AllureSuite("Consumer Tests")]
    public class ApiTest
    {
        private IPactBuilderV3 pact;
        private readonly ApiClient ApiClient;
        private readonly int port = 9000;
        private readonly List<object> products;
        private readonly IMatcher tokenRegex = Match.Regex("Bearer 2022-04-24T11:34:18.045Z", "Bearer \\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}\\.\\d{3}Z");

        public ApiTest(ITestOutputHelper output)
        {
            products = new List<object>()
            {
                new { id = 9, type = "CREDIT_CARD", name = "GEM Visa", version = "v2" },
                new { id = 10, type = "CREDIT_CARD", name = "28 Degrees", version = "v1" }
            };

            var Config = new PactConfig
            {
                PactDir = Path.Join("..", "..", "..", "..", "..", "pacts"),
                LogDir = "pact_logs",
                Outputters = new[] { new XUnitOutput(output) },
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            pact = Pact.V3("ApiClient", "ProductService", Config).UsingNativeBackend(port);
            ApiClient = new ApiClient(new System.Uri($"http://localhost:{port}"));
        }

        [AllureXunit(DisplayName = "Get all products")]
        public async void GetAllProducts()
        {
            // Arange
            pact.UponReceiving("A valid request for all products")
                    .Given("products exist")
                    .WithRequest(HttpMethod.Get, "/api/products")
                    .WithHeader("Authorization", tokenRegex)
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(products));

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetAllProducts();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [AllureXunit(DisplayName = "Get product")]
        public async void GetProduct()
        {
            // Arange
            pact.UponReceiving("A valid request for a product")
                    .Given("product with ID 10 exists")
                    .WithRequest(HttpMethod.Get, "/api/products/10")
                    .WithHeader("Authorization", tokenRegex)
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(products[1]));

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetProduct(10);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [AllureXunit(DisplayName = "No products exists")]
        public async void NoProductsExist()
        {
            // Arange
            pact.UponReceiving("A valid request for all products")
                    .Given("no products exist")
                    .WithRequest(HttpMethod.Get, "/api/products")
                    .WithHeader("Authorization", tokenRegex)
                .WillRespond()
                    .WithStatus(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithJsonBody(new TypeMatcher(new List<object>()));

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetAllProducts();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [AllureXunit(DisplayName = "Product does not exist")]
        public async void ProductDoesNotExist()
        {
            // Arange
            pact.UponReceiving("A valid request for a product")
                    .Given("product with ID 11 does not exist")
                    .WithRequest(HttpMethod.Get, "/api/products/11")
                    .WithHeader("Authorization", tokenRegex)
                .WillRespond()
                    .WithStatus(HttpStatusCode.NotFound);

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetProduct(11);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }

        [AllureXunit(DisplayName = "Get product with missing auth header")]
        public async void GetProductMissingAuthHeader()
        {
            // Arange
            pact.UponReceiving("A valid request for a product")
                    .Given("no auth token is provided")
                    .WithRequest(HttpMethod.Get, "/api/products/10")
                .WillRespond()
                    .WithStatus(HttpStatusCode.Unauthorized);

            await pact.VerifyAsync(async ctx => {
                var response = await ApiClient.GetProduct(10);
                Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            });
        }
    }
}