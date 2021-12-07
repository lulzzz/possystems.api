using System;

namespace POSSystems.Core.Dtos.Session
{
    public class PunchDto
    {
        public DateTime StartTime { get; set; }

        public string StartTimeStr
        {
            get
            {
                return StartTime.ToString("f");
            }
        }

        public DateTime? EndTime { get; set; }

        public string EndTimeStr
        {
            get
            {
                return EndTime?.ToString("f");
            }
        }

        public string SessionTime
        {
            get
            {
                var t = ((EndTime ?? DateTime.Now) - StartTime);
                return string.Format("{0}:{1}:{2}", ((int)t.TotalHours), t.Minutes, t.Seconds);
            }
        }

        public bool OnGoing
        {
            get
            {
                return EndTime == null;
            }
        }
    }
}