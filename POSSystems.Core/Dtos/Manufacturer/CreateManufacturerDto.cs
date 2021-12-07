using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Manufacturer
{
    public class CreateManufacturerDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}