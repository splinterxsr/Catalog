using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Api.Domain.Entities
{
    public class Game
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public DateTime ReleaseDate { get; set; }
        public IList<Genre> Genres { get; set; } = [];
        public decimal Price { get; private set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Constructor for Mapperly mapping. It is required to have a constructor that accepts all properties of the entity for Mapperly to create instances of the entity when mapping from DTOs. This constructor should be public to allow Mapperly to access it when creating instances of the entity during the mapping process, ensuring that all necessary properties are set correctly.
        /// </summary>
        public Game(string name, string description, string publisher, DateTime releaseDate, IList<Genre> genres, decimal price)
        {
            Name = name;
            Description = description;
            Publisher = publisher;
            ReleaseDate = releaseDate;
            Genres = genres;
            Price = price;
        }

        public void Update(string name, string description, string publisher, DateTime releaseDate, decimal price, string status)
        {
            Name = name;
            Description = description;
            Publisher = publisher;
            ReleaseDate = releaseDate;
            Price = price;
            Status = status;
        }
    }

    public class Genre
    {
        public string Name { get; set; } = string.Empty;
    }
}
