using System;

namespace POSSystems.Core.Dtos.Session
{
    public class SessionDto : DtoBase
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Username { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }
    }
}