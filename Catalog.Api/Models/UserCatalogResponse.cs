namespace Catalog.Api.Models
{
    public class UserCatalogResponse
    {
        public int UserId { get; set; }
        public string GameId { get; set; } = string.Empty;
        public DateOnly BuyDate { get; set; }
    }
}