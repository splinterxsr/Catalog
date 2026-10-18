using StackExchange.Redis;
using Testcontainers.Redis;

namespace Catalog.Tests.Api.Fixtures
{
    public abstract class RedisIntegrationTestBase : IAsyncLifetime
    {
        private readonly RedisContainer _redisContainer = new RedisBuilder("redis:latest")
            .Build();

        protected IConnectionMultiplexer RedisConnection { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _redisContainer.StartAsync();
            RedisConnection = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        }

        public async Task DisposeAsync()
        {
            if (RedisConnection != null)
            {
                await RedisConnection.DisposeAsync();
            }
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }
}