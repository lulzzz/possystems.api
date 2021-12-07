using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("session")]
    public class Session : EntityBase
    {
        [Key]
        [Required]
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required, Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime? EndTime { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}