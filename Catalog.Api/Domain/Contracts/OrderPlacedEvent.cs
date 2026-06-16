namespace Catalog.Api.Domain.Contracts
{
    public class OrderPlacedEvent
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public decimal Price { get; set; }

        public OrderPlacedEvent() { }

        public OrderPlacedEvent(int userId, int gameId, decimal price)
        {
            UserId = userId;
            GameId = gameId;
            Price = price;
        }
    }
}