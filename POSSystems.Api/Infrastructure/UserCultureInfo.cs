using POSSystems.Core;
using System;
using TimeZoneConverter;

namespace POSSystems.Web.Infrastructure
{
    public class UserCultureInfo
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _timezone;

        /// <summary>  
        /// Initializes a new instance of the <see cref="UserCultureInfo"/> class.  
        /// </summary>  
        public UserCultureInfo(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _timezone = _unitOfWork.ConfigurationRepository.GetConfigByKey("timezone", "America/Detroit");
            try
            {
                string tz = TZConvert.IanaToWindows(_timezone);
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            }
        }
        /// <summary>  
        /// Gets or sets the time zone.  
        /// </summary>  
        /// <value>  
        /// The time zone.  
        /// </value>  
        public TimeZoneInfo TimeZone { get; set; }
        /// <summary>  
        /// Gets the user local time.  
        /// </summary>  
        /// <returns></returns>  
        public DateTime? GetUserLocalTime(DateTime? dateTime)
        {
            return dateTime.HasValue ? TimeZoneInfo.ConvertTime(dateTime.Value, TimeZone) : dateTime;
        }
        /// <summary>  
        /// Gets the UTC time.  
        /// </summary>  
        /// <param name="datetime">The datetime.</param>  
        /// <returns>Get universal date time based on User's Timezone</returns>  
        public DateTime GetUtcTime(DateTime datetime)
        {
            return TimeZoneInfo.ConvertTime(datetime, TimeZone).ToUniversalTime();
        }
    }


}
