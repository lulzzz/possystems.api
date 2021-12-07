using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("user_session_log")]
    public class UserSessionLog : EntityBase
    {
        [Key]
        [Required]
        [Column("session_id")]
        public Int32? SessionId { get; set; }

        [StringLength(50)]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("login_time")]
        public DateTime? LoginTime { get; set; }

        [Column("logout_time")]
        public DateTime? LogoutTime { get; set; }

        [StringLength(50)]
        [Column("ip_address")]
        public string IpAddress { get; set; }
    }
}