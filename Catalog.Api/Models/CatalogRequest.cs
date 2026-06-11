using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class CatalogRequest
    {
        [Required(ErrorMessage = "The field 'UserId' is mandatory.")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "The field 'GameId' is mandatory.")]
        public int? GameId { get; set; }

        [Required(ErrorMessage = "The field 'Price' is mandatory.")]
        public decimal? Price { get; set; }
    }
}