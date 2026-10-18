using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Models;
using Catalog.Tests.Api.Fixtures;
using FluentAssertions;

namespace Catalog.Tests.Api.Services
{
    public class GameSearchIntegrationTests : ElasticsearchIntegrationTestBase
    {
        private GameSearchService GameSearchService => new(ElasticsearchClient);

        [Fact]
        public async Task IndexAndSearchGameAsync_ShouldIndexAndRetrieveDocument()
        {
            // Arrange
            var game = new GameSearchModel
            {
                Id = "1",
                Name = "Grand Theft Auto VI",
                Description = "Open world game",
                Publisher = "Rockstar",
                Status = "ACTIVE"
            };

            // Act
            await GameSearchService.IndexOrUpdateGameAsync(game);

            // Pequena pausa para o Elasticsearch indexar o documento (refresh)
            await Task.Delay(1000);

            var results = await GameSearchService.SearchAsync("Grand Theft Auto", CancellationToken.None);

            // Assert
            results.Should().NotBeEmpty();
            results.Should().Contain(g => g.Id == "1");
        }
    }
}