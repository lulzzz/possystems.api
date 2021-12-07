using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("company")]
    public class Company : EntityBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [Column("address")]
        public string Address { get; set; }

        [StringLength(100)]
        [Column("address2")]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        [Column("phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(50)]
        [Column("email")]
        public string Email { get; set; }

        [StringLength(50)]
        [Column("website")]
        public string Website { get; set; }

        [StringLength(500)]
        [Column("notes")]
        public string Notes { get; set; }

        [Required]
        [StringLength(50)]
        [Column("smtp_server")]
        public string SmtpServer { get; set; }

        [Required]
        [StringLength(50)]
        [Column("smtp_user")]
        public string SmtpUser { get; set; }

        [Required]
        [StringLength(50)]
        [Column("smtp_password")]
        public string SmtpPassword { get; set; }

        [Column("smtp_port")]
        public int? SmtpPort { get; set; } = 587;

    }
}