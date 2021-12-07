using System;

namespace POSSystems.Core.Dtos.Report
{
    public class TimesheetDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime StartTime { get; set; }

        public string StartTimeStr
        {
            get
            {
                return StartTime.ToString("g");
            }
        }

        public DateTime? EndTime { get; set; }

        public string EndTimeStr
        {
            get
            {
                return EndTime?.ToString("g");
            }
        }

        public TimeSpan TimeDifference
        {
            get
            {
                return ((EndTime ?? DateTime.Now) - StartTime);
            }
        }

        public string TimeDifferenceStr
        {
            get
            {
                return string.Format("{0}:{1}:{2}", ((int)TimeDifference.TotalHours), TimeDifference.Minutes, TimeDifference.Seconds);
            }
        }
    }
}