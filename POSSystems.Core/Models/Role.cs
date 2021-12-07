using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("role")]
    public class Role : EntityBase
    {
        [Key]
        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("role_name")]
        public string RoleName { get; set; }

        [StringLength(500)]
        [Column("description")]
        public string Description { get; set; }
    }
}