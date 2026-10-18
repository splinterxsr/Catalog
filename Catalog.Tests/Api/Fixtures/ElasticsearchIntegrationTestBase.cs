using Elastic.Clients.Elasticsearch;
using Testcontainers.Elasticsearch;

namespace Catalog.Tests.Api.Fixtures
{
    public abstract class ElasticsearchIntegrationTestBase : IAsyncLifetime
    {
        private readonly ElasticsearchContainer _elasticsearchContainer = new ElasticsearchBuilder("elasticsearch:8.11.0")
            .Build();

        protected ElasticsearchClient ElasticsearchClient { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            await _elasticsearchContainer.StartAsync();
            var connectionString = _elasticsearchContainer.GetConnectionString();

            var settings = new ElasticsearchClientSettings(new Uri(connectionString))
                .ServerCertificateValidationCallback((sender, certificate, chain, errors) => true)
                .DefaultIndex("games");

            ElasticsearchClient = new ElasticsearchClient(settings);
        }

        public async Task DisposeAsync()
        {
            await _elasticsearchContainer.StopAsync();
            await _elasticsearchContainer.DisposeAsync();
        }
    }
}