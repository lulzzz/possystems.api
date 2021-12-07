using FluentScheduler;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class Edi832Registry : Registry
    {
        private readonly JobConfig _jobConfig;

        public Edi832Registry(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<Edi832Job> logger, ApplicationData applicationData)
        {
            _jobConfig = jobConfigOptions.Value;

            NonReentrantAsDefault();

            if (_jobConfig.Edi832.DehumanizeTo<JobConfigType>() == JobConfigType.Prod)
            {
                var strEdi832Interval = unitOfWork.ConfigurationRepository.GetConfigByKey("edi832Interval", "1");
                if (!int.TryParse(strEdi832Interval, out int edi832Interval))
                    logger.LogWarning($"edi832_interval is absent in config table");

                if (edi832Interval > 0)
                {
                    Schedule(new Edi832Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunEvery(edi832Interval).Days();
                }
                else
                {
                    Schedule(new Edi832Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
                }
            }
            else if (_jobConfig.Edi832.DehumanizeTo<JobConfigType>() == JobConfigType.Test)
            {
                Schedule(new Edi832Job(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
            }
        }
    }
}