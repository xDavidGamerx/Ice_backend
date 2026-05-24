using System.Threading.Tasks;
using Npgsql;
using Respawn;

namespace IceBackend.IntegrationTests.Fixtures
{
    public static class DatabaseRespawn
    {
        public static async Task ResetAsync(string connectionString)
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = new[] { "public" }
            });
            await respawner.ResetAsync(conn);
        }

        public static async Task DeleteKeysAtomicsAsync(string redisConnectionString, params StackExchange.Redis.RedisKey[] keys)
        {
            if (keys == null || keys.Length == 0) return;

            var muxer = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            var db = muxer.GetDatabase();

            // Usamos FireAndForget para emitir un UNLINK subyacente sin bloquear.
            await db.KeyDeleteAsync(keys, StackExchange.Redis.CommandFlags.FireAndForget);
            
            await muxer.DisposeAsync();
        }
    }
}
