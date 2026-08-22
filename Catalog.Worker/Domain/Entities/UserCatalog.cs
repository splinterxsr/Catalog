using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Worker.Domain.Entities
{
    public class UserCatalog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string GameId { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string OrderId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public DateTime BuyDate { get; set; }

        public UserCatalog(string gameId, string orderId, int userId)
        {
            GameId = gameId;
            OrderId = orderId;
            UserId = userId;
            BuyDate = DateTime.Now;
        }
    }
}