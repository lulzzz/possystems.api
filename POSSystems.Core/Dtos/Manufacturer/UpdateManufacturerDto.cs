using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Manufacturer
{
    public class UpdateManufacturerDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}