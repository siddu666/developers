using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ExchangeRateUpdater.Tests.Integration
{
    public class ApiKeyMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiKeyMiddlewareTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        new KeyValuePair<string, string?>("ApiKey:Enabled", "true"),
                        new KeyValuePair<string, string?>("ApiKey:Value", "test-key")
                    });
                });
            });
        }

        [Fact]
        public async Task Blocks_Request_Without_ApiKey()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/ExchangeRates?currencyCodes=USD");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Allows_Request_With_ApiKey()
        {
            var client = _factory.CreateClient();
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/ExchangeRates?currencyCodes=USD");
            req.Headers.Add("X-API-KEY", "test-key");
            var response = await client.SendAsync(req);

            // We don't assert body here (CNB call); just check it passes middleware
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}



