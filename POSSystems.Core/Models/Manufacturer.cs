using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("manufacturer")]
    public class Manufacturer : EntityBase
    {
        [Key]
        [Required]
        [Column("manufacturer_id")]
        public int ManufacturerId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string Description { get; set; }
    }
}