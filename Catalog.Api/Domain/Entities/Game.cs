namespace Catalog.Api.Domain.Entities
{
    public class Game
    {
        public int Id { get; private set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public DateOnly Release { get; private set; }
        public decimal Price { get; private set; }
        public DateTime LogData { get; private set; }

        /// <summary>
        /// Constructor for Mapperly mapping. It is required to have a constructor that accepts all properties of the entity for Mapperly to create instances of the entity when mapping from DTOs. This constructor should be public to allow Mapperly to access it when creating instances of the entity during the mapping process, ensuring that all necessary properties are set correctly.
        /// </summary>
        public Game(string name, string description, string genre, DateOnly release, decimal price)
        {
            Name = name;
            Description = description;
            Genre = genre;
            Release = release;
            Price = price;
        }

        /// <summary>
        /// Constructor for Entity Framework Core. It is required to have a parameterless constructor for EF Core to create instances of the entity when querying the database. This constructor should be protected or private to prevent it from being used directly in application code, ensuring that the integrity of the entity is maintained through the use of the public constructor that requires all necessary properties to be set.
        /// </summary>
        protected Game()
        {
        }

        public void Update(string name, string description, string genre, DateOnly release, decimal price)
        {
            Name = name;
            Description = description;
            Genre = genre;
            Release = release;
            Price = price;
        }
    }
}
