using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class GameRequest
    {
        [Required(ErrorMessage = "The field 'Name' is mandatory.")]
        [StringLength(50, ErrorMessage = "The field 'Name' must be a maximum of 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "The field 'Description' is mandatory.")]
        [StringLength(200, ErrorMessage = "The field 'Description' must be a maximum of 200 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "The field 'Publisher' is mandatory.")]
        [StringLength(50, ErrorMessage = "The field 'Publisher' must be a maximum of 50 characters.")]
        public string Publisher { get; set; } = string.Empty;
        public IList<GenreRequest> Genres { get; set; } = [];

        [Required(ErrorMessage = "The field 'ReleaseDate' is mandatory.")]
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
    }

    public class GenreRequest
    {
        [Required(ErrorMessage = "The field 'Name' is mandatory.")]
        public string Name { get; set; } = string.Empty;
    }
}