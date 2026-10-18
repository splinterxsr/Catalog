using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using Catalog.Api.Infrastructure.Cache;
using Catalog.Api.Infrastructure.Repositories;
using FluentAssertions;
using Moq;

namespace Catalog.Tests.Api.Repositories
{
    public class CachedCatalogRepositoryTests
    {
        private readonly Mock<ICatalogRepository> _innerMock;
        private readonly Mock<IRedisCacheService> _redisCacheMock;
        private readonly CachedCatalogRepository _cachedRepository;

        public CachedCatalogRepositoryTests()
        {
            _innerMock = new Mock<ICatalogRepository>();
            _redisCacheMock = new Mock<IRedisCacheService>();
            _cachedRepository = new CachedCatalogRepository(_innerMock.Object, _redisCacheMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_UserId_WhenInCache_ReturnsFromCacheWithoutCallingInner()
        {
            // Arrange
            int userId = 1;
            var cachedCatalogs = new List<UserGame>();

            _redisCacheMock.Setup(c => c.GetAsync<IEnumerable<UserGame>>($"catalog:user{userId}"))
                .ReturnsAsync(cachedCatalogs);

            // Act
            var result = await _cachedRepository.GetByIdAsync(userId, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(cachedCatalogs);
            _innerMock.Verify(i => i.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_UserIdAndGameId_WhenInCache_ReturnsFromCacheWithoutCallingInner()
        {
            // Arrange
            int userId = 1;
            string gameId = "game-1";
            var cachedCatalog = Mock.Of<UserCatalog>(uc => uc.UserId == userId && uc.GameId == gameId);

            _redisCacheMock.Setup(c => c.GetAsync<UserCatalog?>($"catalog:user{userId}:game:{gameId}"))
                .ReturnsAsync(cachedCatalog);

            // Act
            var result = await _cachedRepository.GetByIdAsync(userId, gameId, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(cachedCatalog);
            _innerMock.Verify(i => i.GetByIdAsync(userId, gameId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_UserIdAndGameId_WhenNotInCache_FetchesFromInnerAndSetsCache()
        {
            // Arrange
            int userId = 1;
            string gameId = "game-1";
            var catalog = Mock.Of<UserCatalog>(uc => uc.UserId == userId && uc.GameId == gameId);

            _redisCacheMock.Setup(c => c.GetAsync<UserCatalog?>($"catalog:user{userId}:game:{gameId}"))
                .ReturnsAsync((UserCatalog?)null);

            _innerMock.Setup(i => i.GetByIdAsync(userId, gameId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(catalog);

            _redisCacheMock.Setup(c => c.SetAsync($"catalog:user{userId}:game:{gameId}", catalog, null))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _cachedRepository.GetByIdAsync(userId, gameId, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(catalog);
            _innerMock.Verify(i => i.GetByIdAsync(userId, gameId, It.IsAny<CancellationToken>()), Times.Once);
            _redisCacheMock.Verify(c => c.SetAsync($"catalog:user{userId}:game:{gameId}", catalog, null), Times.Once);
        }
    }
}