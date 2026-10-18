using Catalog.Api.Domain.Entities;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Tests.Api.Fixtures;
using FluentAssertions;

namespace Catalog.Tests.Api.Repositories
{
    public class GameRepositoryIntegrationTests : MongoIntegrationTestBase
    {
        private GameRepository GameRepository => new(MongoDatabase);

        [Fact]
        public async Task CreateAsync_InsertsGameIntoCollection()
        {
            // Arrange
            var game = new Game("GTA VI", "Desc", "Publisher", DateTime.UtcNow, [], 79.99m);

            // Act
            await GameRepository.CreateAsync(game, CancellationToken.None);
            var retrievedGame = await GameRepository.GetByIdAsync(game.Id, CancellationToken.None);

            // Assert
            retrievedGame.Should().NotBeNull();
            retrievedGame!.Name.Should().Be("GTA VI");
            retrievedGame.Price.Should().Be(79.99m);
        }

        [Fact]
        public async Task UpdateAsync_ReplacesGameInCollection()
        {
            // Arrange
            var game = new Game("GTA VI", "Desc", "Publisher", DateTime.UtcNow, [], 79.99m);
            await GameRepository.CreateAsync(game, CancellationToken.None);

            var updatedGame = new Game(game.Id, "GTA VI Updated", "Desc", "Publisher", DateTime.UtcNow, [], 89.99m, "ACTIVE");

            // Act
            await GameRepository.UpdateAsync(updatedGame, CancellationToken.None);
            var result = await GameRepository.GetByIdAsync(game.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("GTA VI Updated");
            result.Price.Should().Be(89.99m);
        }

        [Fact]
        public async Task DeleteAsync_DeletesGameFromCollection()
        {
            // Arrange
            var game = new Game("GTA VI", "Desc", "Publisher", DateTime.UtcNow, [], 79.99m);
            await GameRepository.CreateAsync(game, CancellationToken.None);

            // Act
            await GameRepository.DeleteAsync(game.Id, CancellationToken.None);
            var result = await GameRepository.GetByIdAsync(game.Id, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }
    }
}