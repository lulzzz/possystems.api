using FluentScheduler;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class RetailInsightsRegistry : Registry
    {
        private JobConfig _jobConfig;

        public RetailInsightsRegistry(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<RetailInsightsJob> logger, ApplicationData applicationData)
        {
            _jobConfig = jobConfigOptions.Value;

            NonReentrantAsDefault();
            int retailInsightsInterval = 0;

            var strRetailInsightInterval = unitOfWork.ConfigurationRepository.GetConfigByKey("retailInsightsInterval", "7");

            if (!int.TryParse(strRetailInsightInterval, out retailInsightsInterval))
                logger.LogWarning($"retailInsightsInterval is absent in config table");

            if (_jobConfig.RetailInsights.DehumanizeTo<JobConfigType>() == JobConfigType.Prod)
            {
                if (retailInsightsInterval > 0)
                {
                    Schedule(new RetailInsightsJob(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunEvery(retailInsightsInterval).Days();
                }
                else
                {
                    Schedule(new RetailInsightsJob(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
                }
            }
            else if (_jobConfig.RetailInsights.DehumanizeTo<JobConfigType>() == JobConfigType.Test)
            {
                Schedule(new RetailInsightsJob(unitOfWork, jobConfigOptions, logger, applicationData)).ToRunNow();
            }
        }
    }
}