using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Session
{
    public class CreateSessionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}