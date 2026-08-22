using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class GameUpdate
    {
        [Required(ErrorMessage = "The field 'Name' is mandatory.")]
        [StringLength(50, ErrorMessage = "The field 'Name' must be a maximum of 50 characters.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "The field 'Description' is mandatory.")]
        [StringLength(200, ErrorMessage = "The field 'Description' must be a maximum of 200 characters.")]
        public required string Description { get; set; }

        [Required(ErrorMessage = "The field 'Publisher' is mandatory.")]
        [StringLength(50, ErrorMessage = "The field 'Publisher' must be a maximum of 50 characters.")]
        public required string Publisher { get; set; }

        [Required(ErrorMessage = "The field 'ReleaseDate' is mandatory.")]
        public DateTime ReleaseDate { get; set; }

        [Required(ErrorMessage = "The field 'Price' is mandatory.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "The field 'Status' is mandatory.")]
        public string Status { get; set; } = string.Empty;
    }
}
