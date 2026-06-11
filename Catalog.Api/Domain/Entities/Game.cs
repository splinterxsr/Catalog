namespace Catalog.Api.Domain.Entities
{
    public class Game
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public DateOnly Release { get; private set; }
        public decimal Price { get; private set; }
        public DateTime LogData { get; private set; }

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
    }
}
