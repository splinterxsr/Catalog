using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Catalog.Tests.Api.Fixtures
{
    public abstract class MongoIntegrationTestBase : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:latest")
            .Build();

        protected IMongoDatabase MongoDatabase { get; private set; } = null!;
        protected IMongoClient MongoClient { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();
            MongoClient = new MongoClient(_mongoContainer.GetConnectionString());
            MongoDatabase = MongoClient.GetDatabase("catalog_test_db");
        }

        public async Task DisposeAsync()
        {
            await _mongoContainer.StopAsync();
            await _mongoContainer.DisposeAsync();
        }
    }
}