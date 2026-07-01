namespace Catalog.Api.Models
{
    public class UserCatalogResponse
    {
        public int UserId { get; set; }
        public int GameId { get; set; }
        public string? GameName { get; set; }
        public DateOnly BuyDate { get; set; }
    }
}