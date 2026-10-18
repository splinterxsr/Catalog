using Catalog.Api.Infrastructure.Cache;
using Catalog.Tests.Api.Fixtures;
using FluentAssertions;

namespace Catalog.Tests.Api.Cache
{
    public class RedisCacheServiceIntegrationTests : RedisIntegrationTestBase
    {
        private RedisCacheService CacheService => new(RedisConnection);

        [Fact]
        public async Task SetAndGetAsync_ShouldStoreAndRetrieveValue()
        {
            // Arrange
            string key = "game:test-key";
            var expectedValue = new { Name = "GTA VI" };

            // Act
            await CacheService.SetAsync(key, expectedValue);
            var result = await CacheService.GetAsync<object>(key);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task RemoveAsync_DeletesKeyFromRedis()
        {
            // Arrange
            string key = "game:test-key";
            await CacheService.SetAsync(key, "data");

            // Act
            await CacheService.RemoveAsync(key);
            var result = await CacheService.GetAsync<string>(key);

            // Assert
            result.Should().BeNull();
        }
    }
}