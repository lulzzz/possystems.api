using FluentScheduler;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Web.Infrastructure.Downloader;
using POSSystems.Web.Infrastructure.Edi;
using System;
using System.IO;
using System.Linq;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class Edi832Job : IJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JobConfig _jobConfig;
        private readonly ILogger<Edi832Job> _logger;
        private readonly ApplicationData _applicationData;

        public Edi832Job(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<Edi832Job> logger, ApplicationData applicationData)
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
                _logger.LogWarning("There is a job already running. Edi832 stopped.");
                return;
            }

            try
            {
                _applicationData.IsJobRunning = true;

                var jobConfigType = _jobConfig.Edi832.DehumanizeTo<JobConfigType>();
                var sources = _unitOfWork.SourceRepository.GetAllDeferred
                    (s => s.Status == Statuses.Active.Humanize() && s.FileType == FileType.Edi832.Humanize()).ToList();

                foreach (var src in sources)
                {
                    IEdiProcessor iEdiProcessor = null;

                    if (src.Supplier.SupplierName == "Amerisource")
                        iEdiProcessor = new AmeriEdi832Processor(_unitOfWork, _logger);
                    else if (src.Supplier.SupplierName == "RDCDrug")
                        iEdiProcessor = new RdcEdi832Processor(_unitOfWork, _logger);
                    else if (src.Supplier.SupplierName == "Smith Drug Company")
                        iEdiProcessor = new SmithDrugEdi832Processor(_unitOfWork, _logger, jobConfigType);
                    else
                    {
                        _logger.LogWarning($"Source for {src.Supplier.SupplierName} not configured.");
                        continue;
                    }

                    string[] downloadedfiles = null;

                    if (jobConfigType == JobConfigType.Prod)
                    {
                        if (src.Supplier.SupplierName == "Smith Drug Company")
                            new Ftp(_logger, _unitOfWork)
                                .Download(src.HostAddress, src.UserName, src.Password, src.Port, src.HostKey, src.DownloadPath, src.Wildcard, src.LocalPath, src.SubLocalPath, true);
                        else
                            new POSFtp(_logger, _unitOfWork)
                                .Download(src.HostAddress, src.UserName, src.Password, src.Port, src.HostKey, src.DownloadPath, src.Wildcard, src.LocalPath, src.SubLocalPath);
                    }

                    if (jobConfigType == JobConfigType.Prod || jobConfigType == JobConfigType.Test)
                    {
                        var path = Path.Combine(src.LocalPath, src.SubLocalPath);
                        downloadedfiles = Directory.GetFiles(path);
                        if (downloadedfiles == null || !downloadedfiles.Any())
                        {
                            var message = $"No files found in {path} for EDI832.";
                            _logger.LogInformation(message);
                            continue;
                        }

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

                    iEdiProcessor.Process(downloadedfiles, src.LocalPath, src.SubLocalPath, src.ProcessingPath, src.SupplierId, src.FieldSeperator);
                    _logger.LogInformation($"Edi832 merging for supplier: {src.Supplier.SupplierName} completed.");
                }

                _logger.LogInformation("Edi832 merging task completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Edi832 importing error because {ex.Message}");
            }
            finally
            {
                _applicationData.IsJobRunning = false;
            }
        }
    }
}