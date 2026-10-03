namespace Fcg.Contracts
{
    public record OrderPlacedEvent(string Id, int UserId, string UserEmail, string GameId, decimal Price);
}