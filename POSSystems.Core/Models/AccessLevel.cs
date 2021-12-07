using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("access_level")]
    public class AccessLevel : EntityBase
    {
        [Key]
        [Required]
        [Column("access_level_id")]
        public Int32? AccessLevelId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("access_level_type")]
        public string AccessLevelType { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string description { get; set; }

        [StringLength(50)]
        [Column("mdified_by")]
        public string MdifiedBy { get; set; }
    }
}