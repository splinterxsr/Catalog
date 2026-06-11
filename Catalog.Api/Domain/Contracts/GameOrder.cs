namespace Catalog.Api.Domain.Contracts
{
    public class GameOrder
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }

        public GameOrder() { }

        public GameOrder(int userId, int gameId, decimal price)
        {
            UserId = userId;
            GameId = gameId;
            Price = price;
        }
    }
}