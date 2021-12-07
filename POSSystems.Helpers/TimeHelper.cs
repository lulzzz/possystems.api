using System;

namespace POSSystems.Helpers
{
    public static class TimeHelper
    {
        public static string TotalHoursFromSeconds(double seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0}:{1}:{2}", ((int)t.TotalHours), t.Minutes, t.Seconds);
        }
    }
}