namespace Catalog.Api.Models
{
    public class GameResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Genre { get; set; } = null!;
        public DateOnly Release { get; set; }
        public decimal Price { get; set; }
    }
}