using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class GameSearchModel
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Publisher { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public required string Status { get; set; }
    }
}
