using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("role_claim")]
    public class RoleClaim : EntityBase
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("claim_type")]
        public string ClaimType { get; set; }

        [Column("claim_value")]
        public string ClaimValue { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
    }
}