namespace Fcg.Contracts
{
    public record OrderPlacedEvent
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string GameId { get; set; }
        public decimal Price { get; set; }

        public OrderPlacedEvent(string id,int userId, string userEmail, string gameId, decimal price)
        {
            Id = id;
            UserId = userId;
            UserEmail = userEmail;
            GameId = gameId;
            Price = price;
        }
    }
}