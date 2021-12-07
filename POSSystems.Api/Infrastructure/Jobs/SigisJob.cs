using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using POSSystems.Core;
using POSSystems.Core.Dtos;
using POSSystems.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.Jobs
{
    public class SigisJob : IJob
    {
        private readonly string currentFile;
        private readonly string currentDirectory;

        private readonly IUnitOfWork _unitOfWork;
        private readonly AzureConfig _azureConfig;
        private readonly ILogger<SigisJob> _logger;
        private readonly ApplicationData _applicationData;
        private readonly IMapper _mapper;

        public SigisJob(IUnitOfWork unitOfWork, IOptions<AzureConfig> azureConfigOptions, ILogger<SigisJob> logger, ApplicationData applicationData, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _azureConfig = azureConfigOptions?.Value;
            _logger = logger;
            _applicationData = applicationData;
            _mapper = mapper;

            currentDirectory = Directory.GetCurrentDirectory();
            currentFile = Path.Combine(currentDirectory, "Data", "SIGIS_List_CSV.txt");
        }

        public async void Execute()
        {
            if (_applicationData.IsJobRunning)
            {
                string message = "There is a job already running. Sigis stopped.";
                _logger.LogWarning(message: message);
                return;
            }

            _applicationData.IsJobRunning = true;
            string downloadingFile = Path.Combine(currentDirectory, "Data", path3: $"SIGIS_List_CSV_{DateTime.Today:dd-MM-yyyy}.zip");

            try
            {
                await GetSigisFileFromAzure("sigisblob", downloadingFile).ConfigureAwait(false);
                _logger.LogInformation($"Sigis file {downloadingFile} downloaded");

                ZipFile.ExtractToDirectory(downloadingFile, Path.Combine(currentDirectory, "Data"), true);
                _logger.LogInformation($"Sigis file {downloadingFile} extracted");

                using (TextReader textReader = File.OpenText(currentFile))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ",",
                    };
                    using var csv = new CsvReader(textReader, config);

                    var records = csv.GetRecords<EligibleProductCSVDto>().ToList();

                    _unitOfWork.EligibleProductRepository.RepMapper = _mapper;
                    _unitOfWork.EligibleProductRepository.Merge(_mapper.Map<IEnumerable<EligibleProduct>>(records));
                }

                _logger.LogInformation($"Sigis file {downloadingFile} merged in the database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sigis file {downloadingFile} importing error.");
            }
            finally
            {
                _applicationData.IsJobRunning = false;
            }
        }

        private async Task GetSigisFileFromAzure(string blobName, string fileName)
        {
            var accountName = _azureConfig.AzureStorageAccount;
            var keyValue = _azureConfig.AzureStorageAccountKey;
            var useHttps = true;
            var exportSecrets = true;

            var storageCredentials = new StorageCredentials(accountName, keyValue);
            var storageAccount = new CloudStorageAccount(storageCredentials, useHttps);
            var connString = storageAccount.ToString(exportSecrets);

            CloudStorageAccount sa = CloudStorageAccount.Parse(connString);
            CloudBlobClient bc = sa.CreateCloudBlobClient();
            CloudBlobContainer container = bc.GetContainerReference("sigiscontainer");

            await DownloadFileFromAzure(container, blobName, fileName);
        }

        private static async Task DownloadFileFromAzure(CloudBlobContainer container, string blobName, string fileName)
        {
            CloudBlob blob = container.GetBlobReference(blobName);
            await blob.DownloadToFileAsync(fileName, FileMode.Create);
        }
    }
}