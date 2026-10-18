using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Domain.Services;
using Catalog.Api.Infrastructure.Services;
using Catalog.Api.Models;
using FluentAssertions;
using Moq;

namespace Catalog.Tests.Api.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _repositoryMock;
        private readonly Mock<IGameSearchService> _gameSearchServiceMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _repositoryMock = new Mock<IGameRepository>();
            _gameSearchServiceMock = new Mock<IGameSearchService>();
            _gameService = new GameService(_repositoryMock.Object, _gameSearchServiceMock.Object);
        }

        [Fact]
        public async Task AddAsync_WhenValidGame_ShouldSaveToMongoAndIndexInElasticsearch()
        {
            // Arrange
            var game = new Game("game-1", "GTA VI", "Description", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByNameAsync(game.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game?)null);

            _repositoryMock.Setup(r => r.CreateAsync(game, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _gameService.AddAsync(game, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();

            _repositoryMock.Verify(r => r.CreateAsync(game, It.IsAny<CancellationToken>()), Times.Once);
            _gameSearchServiceMock.Verify(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()), Times.Once);
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WhenValidGame_ShouldUpdateMongoAndElasticsearch()
        {
            // Arrange
            var gameId = "game-id-123";
            var existingGame = new Game(gameId, "GTA VI", "Desc", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingGame);

            _repositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game?)null);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _gameService.UpdateAsync(
                gameId, "GTA VI Updated", "New Desc", "Publisher", DateTime.UtcNow, 89.99m, "ACTIVE", CancellationToken.None
            );

            // Assert
            await act.Should().NotThrowAsync();

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
            _gameSearchServiceMock.Verify(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenExistingGame_ShouldDeleteFromMongoAndElasticsearch()
        {
            // Arrange
            var gameId = "game-id-123";
            var existingGame = new Game(gameId, "GTA VI", "Desc", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingGame);

            _repositoryMock.Setup(r => r.DeleteAsync(gameId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.DeleteGameAsync(gameId))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _gameService.DeleteAsync(gameId, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();

            _repositoryMock.Verify(r => r.DeleteAsync(gameId, It.IsAny<CancellationToken>()), Times.Once);
            _gameSearchServiceMock.Verify(s => s.DeleteGameAsync(gameId), Times.Once);
            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task AddAsync_WhenElasticsearchFails_ShouldRollbackMongoCreation()
        {
            // Arrange
            var game = new Game("game-1", "GTA VI", "Description", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByNameAsync(game.Name, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game?)null);

            _repositoryMock.Setup(r => r.CreateAsync(game, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()))
                .ThrowsAsync(new InvalidOperationException("Elasticsearch down"));

            _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _gameService.AddAsync(game, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*O cadastro foi cancelado*");

            _repositoryMock.Verify(r => r.DeleteAsync(game.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WhenElasticsearchFails_ShouldRollbackMongoUpdate()
        {
            // Arrange
            var gameId = "game-id-123";
            var existingGame = new Game(gameId, "GTA VI", "Desc", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingGame);

            _repositoryMock.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Game?)null);

            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.IndexOrUpdateGameAsync(It.IsAny<GameSearchModel>()))
                .ThrowsAsync(new InvalidOperationException("Elasticsearch down"));

            // Act
            Func<Task> act = async () => await _gameService.UpdateAsync(
                gameId, "Novo Nome", "Nova Desc", "Publisher", DateTime.UtcNow, 99.99m, "ACTIVE", CancellationToken.None
            );

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*As alterações foram revertidas*");

            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteAsync_WhenElasticsearchFails_ShouldRollbackMongoDeletion()
        {
            // Arrange
            var gameId = "game-id-123";
            var existingGame = new Game(gameId, "GTA VI", "Desc", "Publisher", DateTime.UtcNow, new List<Genre>(), 79.99m, "ACTIVE");

            _repositoryMock.Setup(r => r.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingGame);

            _repositoryMock.Setup(r => r.DeleteAsync(gameId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _gameSearchServiceMock.Setup(s => s.DeleteGameAsync(gameId))
                .ThrowsAsync(new InvalidOperationException("Elasticsearch down"));

            _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            Func<Task> act = async () => await _gameService.DeleteAsync(gameId, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*A exclusão foi revertida*");

            _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Game>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}