using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models
{
    public class GameRequest
    {
        [Required(ErrorMessage = "The field 'Name' is mandatory.")]
        [StringLength(50, ErrorMessage = "The field 'Name' must be a maximum of 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field 'Description' is mandatory.")]
        [StringLength(200, ErrorMessage = "The field 'Description' must be a maximum of 200 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field 'Genre' is mandatory.")]
        [StringLength(20, ErrorMessage = "The field 'Genre' must be a maximum of 20 characters.")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "The field 'Release' is mandatory.")]
        public DateOnly Release { get; set; }

        public decimal Price { get; set; }
    }
}