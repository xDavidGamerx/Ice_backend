using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IceBackend.Domain.Entities;
using IceBackend.Domain.Enums;
using IceBackend.Infrastructure.Data;
using IceBackend.IntegrationTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Xunit;

namespace IceBackend.IntegrationTests.Auth
{
    [Collection("IntegrationTests")]
    public class SessionTests : IAsyncLifetime
    {
        private readonly WebApiFixture _factory;
        private readonly HttpClient _client;
        private readonly IServiceScope _scope;
        private readonly ApplicationDbContext _db;
        private readonly IConnectionMultiplexer _redis;

        public SessionTests(WebApiFixture factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _redis = _scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
        }

        public async Task InitializeAsync()
        {
            await _db.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            var config = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            
            // Reset de Postgres
            var pgConn = config["ConnectionStrings:PostgresConnection"];
            await DatabaseRespawn.ResetAsync(pgConn!);

            // Reset de Redis
            var endpoints = _redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = _redis.GetServer(endpoint);
                await server.FlushAllDatabasesAsync();
            }
            _scope.Dispose();
        }

        private static string ComputeSha1(string input)
        {
            var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        [Fact]
        public async Task ManageSessions_FullFlow_ShouldSucceed()
        {
            // 1. Registrar usuario para crear su cuenta y obtener sessionToken B
            var registerPayload = new { username = "session_test_user", password = "SecurePassword123!" };
            var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerPayload);
            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var registerBody = await registerResponse.Content.ReadAsStringAsync();
            using var registerDoc = JsonDocument.Parse(registerBody);
            var playerId = registerDoc.RootElement.GetProperty("playerId").GetProperty("value").GetGuid();
            var tokenB = registerDoc.RootElement.GetProperty("sessionToken").GetString();
            Assert.NotNull(tokenB);

            // 2. Sembrar de forma manual el token A (simulando una sesion concurrente previa o en otro dispositivo)
            // para evitar que la rotacion de sesiones de AuthService (en login) invalide el token B.
            var tokenA = "session_token_concurrente_A";
            var cache = _scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var db = _redis.GetDatabase();

            await cache.SetAsync($"session:token:{tokenA}", Encoding.UTF8.GetBytes(playerId.ToString()), 
                new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) 
                });

            double expireScore = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
            await db.SortedSetAddAsync($"player:sessions:{playerId}", tokenA, expireScore);

            // Calculamos hashes
            var hashA = ComputeSha1(tokenA);
            var hashB = ComputeSha1(tokenB);

            // 3. Listar sesiones usando token B (que ahora es el actual guardado en session:player:{id})
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
            var sessionsResponse = await _client.GetAsync("/api/v1/auth/sessions");
            Assert.Equal(HttpStatusCode.OK, sessionsResponse.StatusCode);

            var sessionsBody = await sessionsResponse.Content.ReadAsStringAsync();
            using var sessionsDoc = JsonDocument.Parse(sessionsBody);
            var sessionsList = sessionsDoc.RootElement.GetProperty("sessions");
            Assert.Equal(2, sessionsList.GetArrayLength());

            bool foundA = false;
            bool foundB = false;

            foreach (var session in sessionsList.EnumerateArray())
            {
                var hash = session.GetProperty("tokenHash").GetString();
                var isCurrent = session.GetProperty("isCurrent").GetBoolean();

                if (hash == hashA)
                {
                    foundA = true;
                    // El token actual de la solicitud es B, no A. Así que isCurrent para A debe ser false.
                    Assert.False(isCurrent);
                }
                else if (hash == hashB)
                {
                    foundB = true;
                    // El token actual de la solicitud es B, por lo que isCurrent para B debe ser true.
                    Assert.True(isCurrent);
                }
            }

            Assert.True(foundA);
            Assert.True(foundB);

            // 4. Eliminar sesión antigua A (usando token B)
            var deleteResponse = await _client.DeleteAsync($"/api/v1/auth/sessions/{hashA}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            var deleteBody = await deleteResponse.Content.ReadAsStringAsync();
            using var deleteDoc = JsonDocument.Parse(deleteBody);
            Assert.Equal("Sesión cerrada exitosamente.", deleteDoc.RootElement.GetProperty("message").GetString());

            // 5. Verificar que el token A ya no funciona
            var clientA = _factory.CreateClient();
            clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var sessionsResponseA = await clientA.GetAsync("/api/v1/auth/sessions");
            Assert.Equal(HttpStatusCode.Unauthorized, sessionsResponseA.StatusCode);

            // 6. Eliminar la propia sesión actual (token B)
            var deleteCurrentResponse = await _client.DeleteAsync($"/api/v1/auth/sessions/{hashB}");
            Assert.Equal(HttpStatusCode.OK, deleteCurrentResponse.StatusCode);

            // 7. Verificar que el token B ya no funciona
            var sessionsResponseBAfter = await _client.GetAsync("/api/v1/auth/sessions");
            Assert.Equal(HttpStatusCode.Unauthorized, sessionsResponseBAfter.StatusCode);
        }

        [Fact]
        public async Task GetSessions_Unauthenticated_ShouldReturnUnauthorized()
        {
            var response = await _client.GetAsync("/api/v1/auth/sessions");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteSession_Unauthenticated_ShouldReturnUnauthorized()
        {
            var response = await _client.DeleteAsync("/api/v1/auth/sessions/anyhash");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
