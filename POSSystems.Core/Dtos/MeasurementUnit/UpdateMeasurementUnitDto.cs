using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.MeasurementUnit
{
    public class UpdateMeasurementUnitDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string MeasurementName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }
}