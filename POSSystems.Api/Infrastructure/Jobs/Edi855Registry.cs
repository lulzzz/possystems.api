using FluentScheduler;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class Edi855Registry : Registry
    {
        private readonly JobConfig _jobConfig;

        public Edi855Registry(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<Edi855Job> logger, ApplicationData applicationData)
        {
            _jobConfig = jobConfigOptions.Value;

            NonReentrantAsDefault();

            #region 855 Job

            if (_jobConfig.Edi855.DehumanizeTo<JobConfigType>() == JobConfigType.Prod)
            {
                var strEdi855Interval = unitOfWork.ConfigurationRepository.GetConfigByKey("edi855Interval", "1");

                if (!int.TryParse(strEdi855Interval, out int edi855Interval))
                    logger.LogWarning($"edi855_interval is absent in config table");

                if (edi855Interval > 0)
                {
                    Schedule(new Edi855Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunEvery(edi855Interval).Minutes();
                }
                else
                {
                    Schedule(new Edi855Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
                }
            }
            else if (_jobConfig.Edi855.DehumanizeTo<JobConfigType>() == JobConfigType.Test)
            {
                Schedule(new Edi855Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
            }

            #endregion 855 Job
        }
    }
}