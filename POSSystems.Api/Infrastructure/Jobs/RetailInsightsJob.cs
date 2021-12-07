using FluentScheduler;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Downloader;
using POSSystems.Web.Infrastructure.Edi;
using System;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class RetailInsightsJob : IJob
    {
        private IUnitOfWork _unitOfWork;
        private JobConfig _jobConfig;
        private ILogger<RetailInsightsJob> _logger;
        private ApplicationData _applicationData;

        public RetailInsightsJob(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<RetailInsightsJob> logger, ApplicationData applicationData)
        {
            _unitOfWork = unitOfWork;
            _jobConfig = jobConfigOptions.Value;
            _logger = logger;
            _applicationData = applicationData;
        }

        public void Execute()
        {
            if (_applicationData.IsJobRunning)
            {
                _logger.LogWarning($"There is a job already running. RetailInsights stopped.");
                return;
            }

            _applicationData.IsJobRunning = true;

            try
            {
                var retailInsightsProcessor = new RetailInsightsProcessor(_unitOfWork);

                var sources = _unitOfWork.SourceRepository.GetAllDeferred(s => s.Status == Statuses.Active.Humanize() && s.FileType == FileType.ZIP.Humanize()).ToList();

                var ftpRetailInsights = new POSFtp(_logger, _unitOfWork);
                foreach (var src in sources)
                {
                    string[] downloadedfiles = null;

                    if (_jobConfig.RetailInsights.DehumanizeTo<JobConfigType>() == JobConfigType.Prod)
                    {
                        downloadedfiles = ftpRetailInsights.Download(src.HostAddress, src.UserName, src.Password, src.Port, src.HostKey, src.DownloadPath, src.Wildcard, src.LocalPath, src.SubLocalPath);

                        foreach (var df in downloadedfiles)
                        {
                            var batchFile = new BatchFile
                            {
                                FileName = df,
                                Status = FileStatus.Downloaded.Humanize(),
                                SupplierId = src.SupplierId,
                                CreateDate = DateTime.Now
                            };

                            _unitOfWork.BatchFileRepository.Add(batchFile);

                            if (!_unitOfWork.Save())
                            {
                                _logger.LogError($"File {batchFile.FileName} was not inserted.");
                            }
                        }
                    }

                    retailInsightsProcessor.Process(downloadedfiles, src.LocalPath, src.SubLocalPath, src.ProcessingPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RetailInsights importing error because {ex.Message}");
            }
            finally
            {
                _applicationData.IsJobRunning = false;
            }
        }
    }
}