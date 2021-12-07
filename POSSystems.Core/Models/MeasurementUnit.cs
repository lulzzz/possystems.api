using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("measurement_unit")]
    public class MeasurementUnit : EntityBase
    {
        [Key]
        [Required]
        [Column("measurement_id")]
        public int MeasurementId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("measurement_name")]
        public string MeasurementName { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string Description { get; set; }
    }
}