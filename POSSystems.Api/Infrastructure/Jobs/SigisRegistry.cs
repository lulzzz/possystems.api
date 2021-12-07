using AutoMapper;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class SigisRegistry : Registry
    {
        public SigisRegistry(IUnitOfWork unitOfWork, IOptions<AzureConfig> azureConfigOptions, ILogger<SigisJob> logger, ApplicationData applicationData, IMapper mapper)
        {
            NonReentrantAsDefault();

            var sigisInterval = 0;

            var strSigisInterval = unitOfWork.ConfigurationRepository.GetConfigByKey("sigisInterval", "15");
            if (!int.TryParse(strSigisInterval, out sigisInterval))
                logger.LogWarning("sigis_interval is absent in config table.");

            if (sigisInterval > 0)
            {
                Schedule(new SigisJob(unitOfWork, azureConfigOptions, logger, applicationData, mapper)).ToRunEvery(1).Months().On(sigisInterval);
            }
            else
            {
                Schedule(new SigisJob(unitOfWork, azureConfigOptions, logger, applicationData, mapper)).ToRunNow();
            }
        }
    }
}