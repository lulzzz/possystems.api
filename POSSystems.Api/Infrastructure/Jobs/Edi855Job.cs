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
    public class Edi855Job : IJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JobConfig _jobConfig;
        private readonly ILogger<Edi855Job> _logger;
        private readonly ApplicationData _applicationData;

        public Edi855Job(IUnitOfWork unitOfWork, IOptions<JobConfig> jobConfigOptions, ILogger<Edi855Job> logger, ApplicationData applicationData)
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
                _logger.LogWarning("There is a job already running. Edi855Job stopped.");
                return;
            }

            _applicationData.IsJobRunning = true;

            try
            {
                var jobConfigType = _jobConfig.Edi855.DehumanizeTo<JobConfigType>();
                var sources = _unitOfWork.SourceRepository.GetAllDeferred(s => s.Status == Statuses.Active.Humanize() && s.FileType == FileType.Edi855.Humanize()).ToList();

                foreach (var src in sources)
                {
                    IEdiProcessor ediProcessor = null;
                    if (src.Supplier.SupplierName == "Amerisource")
                        ediProcessor = new Edi855Processor(_unitOfWork, _logger);
                    else if (src.Supplier.SupplierName == "Smith Drug Company")
                        ediProcessor = new SmithDrugEdi855Processor(_unitOfWork, _logger, jobConfigType);
                    else
                    {
                        _logger.LogWarning($"Source for {src.Supplier.SupplierName} not configured.");
                        continue;
                    }

                    string[] downloadedfiles = null;
                    string[] remoteFiles = null;

                    IDownloader downloader = null;
                    if (src.Supplier.SupplierName == "Smith Drug Company")
                        downloader = new Ftp(_logger, _unitOfWork);
                    else
                        downloader = new POSFtp(_logger, _unitOfWork);

                    //download files and save to batch
                    if (jobConfigType == JobConfigType.Prod)
                    {
                        remoteFiles = downloader.Download(src.HostAddress, src.UserName, src.Password, src.Port, src.HostKey, src.DownloadPath, src.Wildcard, src.LocalPath, src.SubLocalPath);
                    }

                    if (jobConfigType == JobConfigType.Prod || jobConfigType == JobConfigType.Test)
                    {
                        // get manual file upload to dir incase of system failure
                        downloadedfiles = Directory.GetFiles(Path.Combine(src.LocalPath, src.SubLocalPath));
                        if (downloadedfiles != null && downloadedfiles.Any())
                        {
                            foreach (var df in downloadedfiles)
                            {
                                var batchFile = new BatchFile
                                {
                                    FileName = df,
                                    Status = FileStatus.Downloaded.Humanize(),
                                    SupplierId = src.SupplierId,
                                    ErrorCount = 0,
                                    FileId = 0,
                                    CreateDate = DateTime.Now
                                };

                                _unitOfWork.BatchFileRepository.Add(batchFile);
                                if (!_unitOfWork.Save())
                                {
                                    _logger.LogError($"File {batchFile.FileName} was not inserted.");
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"No files found for EDI855.");
                            continue;
                        }
                    }

                    if (downloadedfiles != null && downloadedfiles.Any())
                    {
                        ediProcessor.Process(downloadedfiles, src.LocalPath, src.SubLocalPath, src.ProcessingPath, src.SupplierId, src.FieldSeperator);
                        if (jobConfigType == JobConfigType.Prod)
                        {
                            if (remoteFiles != null && remoteFiles.Any())
                            {
                                downloader.DeleteFiles(remoteFiles, src.HostAddress, src.UserName, src.Password, src.Port, src.HostKey, src.DownloadPath);
                            }
                        }
                    }

                    _logger.LogInformation($"Edi855 merging for supplier: {src.Supplier.SupplierName} completed.");
                }

                _logger.LogInformation("Edi855 merging task completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Edi855 importing error because {ex.Message}");
            }
            finally
            {
                _applicationData.IsJobRunning = false;
            }
        }
    }
}