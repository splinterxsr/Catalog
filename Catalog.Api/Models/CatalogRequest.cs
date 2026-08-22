using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class CatalogRequest
    {
        [Required(ErrorMessage = "The field 'UserId' is mandatory.")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "The field 'UserEmail' is mandatory.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "The field 'UserEmail' must be a valid email address.")]
        public string? UserEmail { get; set; }

        [Required(ErrorMessage = "The field 'GameId' is mandatory.")]
        public string GameId { get; set; } = string.Empty;

        [Required(ErrorMessage = "The field 'Price' is mandatory.")]
        public decimal? Price { get; set; }
    }
}