using Catalog.Api.Domain.Entities;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Tests.Api.Fixtures;
using FluentAssertions;
using MongoDB.Bson;

namespace Catalog.Tests.Api.Repositories
{
    public class OrderRepositoryIntegrationTests : MongoIntegrationTestBase
    {
        private OrderRepository OrderRepository => new(MongoDatabase);

        [Fact]
        public async Task AddAndGetOrderByIdAsync_InsertsAndRetrievesOrderFromMongo()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new GameOrder
            {
                Id = orderId.ToString(),
                UserId = 1,
                GameId = ObjectId.GenerateNewId().ToString(),
                Price = 100m
            };

            // Act
            await OrderRepository.AddAsync(order, CancellationToken.None);
            var retrievedOrder = await OrderRepository.GetOrderByIdAsync(orderId, CancellationToken.None);

            // Assert
            retrievedOrder.Should().NotBeNull();
            retrievedOrder!.Id.Should().Be(orderId.ToString());
            retrievedOrder.Price.Should().Be(100m);
            retrievedOrder.UserId.Should().Be(1);
        }
    }
}