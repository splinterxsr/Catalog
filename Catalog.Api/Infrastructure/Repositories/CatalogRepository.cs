using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Api.Infrastructure.Repositories
{
    public class CatalogRepository(IMongoDatabase database) : ICatalogRepository
    {
        private readonly string _collectionName = "catalogs";

        public async Task<IEnumerable<UserGame>> GetByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var catalogsCollection = database.GetCollection<BsonDocument>(_collectionName);

            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument("UserId", userId)),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "games" },
                    { "localField", "GameId" },
                    { "foreignField", "_id" },
                    { "as", "game" }
                }),
                new BsonDocument("$unwind", "$game"),
                new BsonDocument("$lookup", new BsonDocument
                {
                    { "from", "orders" },
                    { "localField", "OrderId" },
                    { "foreignField", "_id" },
                    { "as", "order" }
                }),
                new BsonDocument("$unwind", "$order"),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "UserId", 1 },
                    { "Game", "$game.Name" },
                    { "PricePaid", "$order.Price" },
                    { "BuyDate", 1 }
                })
            };

            var result = await catalogsCollection.Aggregate<UserGame>(pipeline).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<UserCatalog?> GetByIdAsync(int userId, string gameId, CancellationToken cancellationToken) => await database.GetCollection<UserCatalog>(_collectionName).Find(uc => uc.UserId == userId && uc.GameId == gameId).FirstOrDefaultAsync(cancellationToken);
    }
}