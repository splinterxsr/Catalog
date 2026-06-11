namespace Catalog.Api.Domain.Entities
{
    public class UserCatalog
    {
        public int Id { get; private set; }
        public int GameId { get; private set; }
        public int UserId { get; private set; }
        public DateOnly BuyDate { get; private set; }
        public Game Game { get; private set; } = null!;
        public DateTime LogData { get; private set; }

        /// <summary>
        /// Constructor for Entity Framework Core. It is required to have a parameterless constructor for EF Core to create instances of the entity when querying the database. This constructor should be protected or private to prevent it from being used directly in application code, ensuring that the integrity of the entity is maintained through the use of the public constructor that requires all necessary properties to be set.
        /// </summary>
        protected UserCatalog()
        { 
        }
    }
}
