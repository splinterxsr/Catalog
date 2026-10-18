using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Cache;
using Catalog.Api.Infrastructure.Repositories;
using FluentAssertions;
using Moq;

namespace Catalog.Tests.Api.Repositories
{
    public class CachedGameRepositoryTests
    {
        private readonly Mock<IGameRepository> _innerMock;
        private readonly Mock<IRedisCacheService> _redisCacheMock;
        private readonly CachedGameRepository _cachedRepository;

        public CachedGameRepositoryTests()
        {
            _innerMock = new Mock<IGameRepository>();
            _redisCacheMock = new Mock<IRedisCacheService>();
            _cachedRepository = new CachedGameRepository(_innerMock.Object, _redisCacheMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemInCache_ReturnsFromCacheWithoutCallingInner()
        {
            var gameId = "1";
            var cachedGame = new Game(gameId, "GTA VI", "Desc", "Pub", DateTime.UtcNow, [], 70m, "ACTIVE");

            _redisCacheMock.Setup(c => c.GetAsync<Game?>($"game:id:{gameId}"))
                .ReturnsAsync(cachedGame);

            var result = await _cachedRepository.GetByIdAsync(gameId, CancellationToken.None);

            result.Should().BeEquivalentTo(cachedGame);
            _innerMock.Verify(i => i.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WhenItemNotInCache_FetchesFromInnerAndSetsCache()
        {
            var gameId = "1";
            var game = new Game(gameId, "GTA VI", "Desc", "Pub", DateTime.UtcNow, [], 70m, "ACTIVE");

            _redisCacheMock.Setup(c => c.GetAsync<Game?>($"game:id:{gameId}"))
                .ReturnsAsync((Game?)null);

            _innerMock.Setup(i => i.GetByIdAsync(gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(game);

            _redisCacheMock.Setup(c => c.SetAsync($"game:id:{gameId}", game, null))
                .Returns(Task.CompletedTask);

            var result = await _cachedRepository.GetByIdAsync(gameId, CancellationToken.None);

            result.Should().BeEquivalentTo(game);
            _innerMock.Verify(i => i.GetByIdAsync(gameId, It.IsAny<CancellationToken>()), Times.Once);
            _redisCacheMock.Verify(c => c.SetAsync($"game:id:{gameId}", game, null), Times.Once);
        }
    }
}