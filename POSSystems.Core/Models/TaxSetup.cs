using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("tax_setup")]
    public class TaxSetup : EntityBase
    {
        [Key]
        [Required]
        [StringLength(50)]
        [Column("statecode")]
        public string Statecode { get; set; }

        [Required]
        [StringLength(100)]
        [Column("state_name")]
        public string StateName { get; set; }

        [Required]
        [Column("tax_rate")]
        public double? TaxRate { get; set; }
    }
}