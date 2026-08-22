using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Api.Domain.Entities
{
    public class UserCatalog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string GameId { get; private set; } = string.Empty;
        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; private set; } = string.Empty;
        public int UserId { get; private set; }
        public DateTime BuyDate { get; private set; }
    }

    public class UserGame
    {
        public int UserId { get; set; }
        public string Game { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
        public DateTime BuyDate { get; set; }
    }
}