namespace Catalog.Api.Domain.Entities
{
    public class GameOrder
    {
        public int UserId { get; private set; }
        public int GameId { get; private set; }
        public decimal Price { get; private set; }

        public GameOrder(int userId, int gameId, decimal price)
        {
            UserId = userId;
            GameId = gameId;
            Price = price;
        }
    }
}