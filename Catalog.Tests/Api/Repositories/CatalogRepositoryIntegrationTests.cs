using Catalog.Api.Domain.Entities;
using Catalog.Api.Infrastructure.Repositories;
using Catalog.Tests.Api.Fixtures;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Catalog.Tests.Api.Repositories
{
    public class CatalogRepositoryIntegrationTests : MongoIntegrationTestBase
    {
        private GameRepository GameRepository => new(MongoDatabase);
        private CatalogRepository CatalogRepository => new(MongoDatabase);
        private OrderRepository OrderRepository => new(MongoDatabase);

        static CatalogRepositoryIntegrationTests()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(GameOrder)))
            {
                BsonClassMap.RegisterClassMap<GameOrder>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id)
                      .SetSerializer(new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.String));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Game)))
            {
                BsonClassMap.RegisterClassMap<Game>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id)
                      .SetSerializer(new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.String));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserCatalog)))
            {
                BsonClassMap.RegisterClassMap<UserCatalog>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id)
                      .SetSerializer(new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.String));

                    cm.GetMemberMap(c => c.OrderId)
                      .SetSerializer(new MongoDB.Bson.Serialization.Serializers.StringSerializer(BsonType.String));

                    cm.SetIgnoreExtraElements(true);
                });
            }
        }

        [Fact]
        public async Task GetByIdAsync_UserId_ReturnsUserGamesFromAggregationPipeline()
        {
            // Arrange
            int userId = 1;
            var gameId = ObjectId.GenerateNewId().ToString();
            Guid orderId = Guid.NewGuid();

            var game = new Game(gameId, "Cyberpunk 2077", "RPG", "CDPR", DateTime.UtcNow, [], 199.99m, "ACTIVE");
            await GameRepository.CreateAsync(game, CancellationToken.None);

            var order = new GameOrder { Id = orderId.ToString(), UserId = userId, GameId = gameId, Price = 199.99m };
            await OrderRepository.AddAsync(order, CancellationToken.None);

            var userCatalogDocument = new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "UserId", userId },
                { "GameId", gameId },
                { "OrderId", orderId.ToString() },
                { "BuyDate", DateTime.UtcNow }
            };

            await MongoDatabase.GetCollection<BsonDocument>("catalogs").InsertOneAsync(userCatalogDocument);

            // Act
            var result = await CatalogRepository.GetByIdAsync(userId, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            var item = result.First();
            item.UserId.Should().Be(userId);
            item.Game.Should().Be("Cyberpunk 2077");
            item.PricePaid.Should().Be(199.99m);
        }

        [Fact]
        public async Task GetByIdAsync_UserIdAndGameId_ReturnsUserCatalog()
        {
            // Arrange
            int userId = 1;
            var gameId = ObjectId.GenerateNewId().ToString();
            string orderId = Guid.NewGuid().ToString();

            var userCatalog = Activator.CreateInstance(typeof(UserCatalog), true)!;

            typeof(UserCatalog).GetProperty("UserId")?.SetValue(userCatalog, userId);
            typeof(UserCatalog).GetProperty("GameId")?.SetValue(userCatalog, gameId);
            typeof(UserCatalog).GetProperty("OrderId")?.SetValue(userCatalog, orderId);
            typeof(UserCatalog).GetProperty("BuyDate")?.SetValue(userCatalog, DateTime.UtcNow);

            await MongoDatabase.GetCollection<UserCatalog>("catalogs").InsertOneAsync((UserCatalog)userCatalog);

            // Act
            var result = await CatalogRepository.GetByIdAsync(userId, gameId, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.GameId.Should().Be(gameId);
        }
    }
}