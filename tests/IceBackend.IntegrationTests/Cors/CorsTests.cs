using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IceBackend.IntegrationTests.Fixtures;
using Xunit;

namespace IceBackend.IntegrationTests.Cors
{
    [Collection("IntegrationTests")]
    public class CorsTests : IAsyncLifetime
    {
        private readonly WebApiFixture _factory;
        private readonly HttpClient _client;

        public CorsTests(WebApiFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => Task.CompletedTask;

        [Theory]
        [InlineData("http://localhost:3000")]
        [InlineData("https://launcher.ice.gg")]
        public async Task Options_AllowedOrigin_ShouldReturnCorsHeaders(string origin)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/subscription/me");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "GET");
            request.Headers.Add("Access-Control-Request-Headers", "Authorization");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            // ASP.NET Core usualmente retorna 204 No Content para preflight exitosos
            Assert.True(response.StatusCode == HttpStatusCode.NoContent || response.StatusCode == HttpStatusCode.OK);

            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.Equal(origin, response.Headers.GetValues("Access-Control-Allow-Origin").First());
            Assert.True(response.Headers.Contains("Access-Control-Allow-Credentials"));
            Assert.Equal("true", response.Headers.GetValues("Access-Control-Allow-Credentials").First());
        }

        [Fact]
        public async Task Options_DisallowedOrigin_ShouldNotReturnCorsHeaders()
        {
            // Arrange
            var origin = "http://malicious-site.gg";
            var request = new HttpRequestMessage(HttpMethod.Options, "/api/v1/subscription/me");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "GET");
            request.Headers.Add("Access-Control-Request-Headers", "Authorization");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            // Para orígenes no permitidos, no se deben incluir las cabeceras CORS de respuesta
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.False(response.Headers.Contains("Access-Control-Allow-Credentials"));
        }
    }
}
