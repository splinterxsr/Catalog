namespace Catalog.Api.Domain.Contracts
{
    public record OrderPlacedEvent
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public int GameId { get; set; }
        public decimal Price { get; set; }

        public OrderPlacedEvent(int userId, string userEmail, int gameId, decimal price)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            UserEmail = userEmail;
            GameId = gameId;
            Price = price;
        }
    }
}