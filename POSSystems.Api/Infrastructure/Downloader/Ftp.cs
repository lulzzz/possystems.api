using Microsoft.Extensions.Logging;
using POSSystems.Core;
using System;
using System.Collections.Generic;
using System.IO;
using WinSCP;

namespace POSSystems.Web.Infrastructure.Downloader
{
    public class Ftp : IDownloader
    {
        private ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public Ftp(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public string[] Download(string hostAddress, string userName, string password, int port,
            string hostKey, string downloadPath, string wildcardFileName,
            string outputPath, string subOutputPath = "", bool deleteFile = false)
        {
            var downloadedfiles = new List<string>();

            string downloadFile, inputFile = "";

            if (!Directory.Exists(Path.Combine(outputPath, subOutputPath)))
                Directory.CreateDirectory(Path.Combine(outputPath, subOutputPath));

            try
            {
                // Define sessins variable
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = hostAddress,
                    UserName = userName,
                    Password = password,
                    PortNumber = port,
                };

                // Instanciate WinSCP object here
                using (var session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload files
                    var transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;
                    transferOptions.FilePermissions = null;
                    transferOptions.PreserveTimestamp = false;
                    transferOptions.ResumeSupport.State = TransferResumeSupportState.Off;

                    var directory = session.ListDirectory(downloadPath);

                    foreach (RemoteFileInfo fileInfo in directory.Files)
                    {
                        downloadFile = fileInfo.Name;

                        if (downloadFile.ToUpperInvariant().Contains(wildcardFileName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            inputFile = Path.Combine(outputPath, subOutputPath, downloadFile);

                            // Download the File from SFTP Server
                            var transferResult = session.GetFiles(fileInfo.FullName, inputFile, deleteFile, transferOptions);

                            // Throw on any error
                            transferResult.Check();

                            // Remove server file
                            // session.RemoveFiles(fileInfo.FullName);
                            downloadedfiles.Add(fileInfo.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Not able to download file from source {hostAddress} for {ex.Message}.");
            }

            return downloadedfiles.ToArray();
        }

        public void DeleteFiles(string[] downloadedfileNames, string hostAddress, string userName, string password, int port, string hostKey, string downloadPath)
        {
            try
            {
                // Define sessins variable
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = hostAddress,
                    UserName = userName,
                    Password = password,
                    PortNumber = port,
                };

                using (var session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);
                    _logger.LogInformation($"Connected to {hostAddress}.");

                    foreach (var ftpPath in downloadedfileNames)
                    {
                        session.RemoveFiles(ftpPath);
                        _logger.LogInformation($"Deleted {ftpPath}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Not able to delete data from Edi 855 source {hostAddress} for {ex.Message}.");
            }
        }
    }
}