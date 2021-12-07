using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("user")]
    public class User : EntityBase
    {
        [Key]
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("user_name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(150)]
        [Column("password")]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        [Column("display_name")]
        public string DisplayName { get; set; }

        [StringLength(50)]
        [Column("full_name")]
        public string FullName { get; set; }

        [StringLength(50)]
        [Column("designation")]
        public string Designation { get; set; }

        [Required]
        [StringLength(50)]
        [Column("contact_no")]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(50)]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(200)]
        [Column("address")]
        public string Address { get; set; }

        [Column("barcode")]
        public string Barcode { get; set; }

        [Column("company_id")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}