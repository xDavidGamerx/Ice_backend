using Polly;
using Polly.Timeout;
using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Xunit;

namespace IceBackend.IntegrationTests.Fixtures
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:15").Build();

        private readonly RedisContainer _redis = new RedisBuilder("redis:7").Build();

        public string PostgresConnectionString => _pg.GetConnectionString();
        public string RedisConnectionString => _redis.GetConnectionString();

        public async Task InitializeAsync()
        {
            var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30), TimeoutStrategy.Pessimistic);
            var retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            var resiliencePolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

            await Task.WhenAll(
                resiliencePolicy.ExecuteAsync(() => _pg.StartAsync()),
                resiliencePolicy.ExecuteAsync(() => _redis.StartAsync())
            );
        }

        public async Task DisposeAsync()
        {
            await _pg.DisposeAsync();
            await _redis.DisposeAsync();
        }
    }
}
