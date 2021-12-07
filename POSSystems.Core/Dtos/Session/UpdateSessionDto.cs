using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Session
{
    public class UpdateSessionDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}