using Microsoft.Extensions.Logging;
using POSSystems.Core;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;

namespace POSSystems.Web.Infrastructure.Downloader
{
    public class POSFtp : IDownloader
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public POSFtp(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public string[] Download(string hostAddress, string userName, string password, int port,
            string hostKey, string downloadPath, string wildcardFileName,
            string outputPath, string subOutputPath = "", bool deleteFile = false)
        {
            var downloadedFiles = new List<string>();

            string downloadFile, remoteFile, inputFile;

            string fullOutputPath = Path.Combine(outputPath, subOutputPath);
            if (!Directory.Exists(fullOutputPath))
                Directory.CreateDirectory(fullOutputPath);

            var connectionInfo = new ConnectionInfo(hostAddress,
                                    port,
                                    userName,
                                    new PasswordAuthenticationMethod(userName, password),
                                    new PrivateKeyAuthenticationMethod(hostKey));

            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    _logger.LogInformation($"Connected to {hostAddress}.");

                    var files = client.ListDirectory(downloadPath);

                    foreach (SftpFile file in files)
                    {
                        if (!_unitOfWork.BatchFileRepository.Exists(bf => bf.FileName.Contains(file.Name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            downloadFile = file.Name;

                            if (downloadFile.Contains(wildcardFileName, StringComparison.OrdinalIgnoreCase))
                            {
                                remoteFile = Path.Combine(downloadPath, downloadFile);
                                inputFile = Path.Combine(outputPath, subOutputPath, downloadFile);

                                if (File.Exists(inputFile))
                                    File.Delete(inputFile);

                                using (var ms = new MemoryStream())
                                {
                                    _logger.LogInformation($"Downloading ... {file.Name} of size {file.Length}");

                                    client.DownloadFile(remoteFile, ms);

                                    //You have to rewind the MemoryStream before copying
                                    ms.Seek(0, SeekOrigin.Begin);

                                    using (var fs = new FileStream(inputFile, FileMode.OpenOrCreate))
                                    {
                                        ms.CopyTo(fs);
                                        fs.Flush();

                                        downloadedFiles.Add(file.FullName);
                                        _logger.LogInformation($"Edi 832 file {inputFile} downloaded.");
                                    }
                                }
                            }
                        }
                    }

                    client.Disconnect();
                }
                catch (SshAuthenticationException sae)
                {
                    _logger.LogError($"Not able to login to {hostAddress} because {sae.Message}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Not able to import data from Edi source {hostAddress} for {ex.Message}.");
                }
            }

            return downloadedFiles.ToArray();
        }

        public void DeleteFiles(string[] downloadedfileNames, string hostAddress, string userName, string password, int port, string hostKey, string downloadPath)
        {
            var connectionInfo = new ConnectionInfo(hostAddress,
                                        port,
                                        userName,
                                        new PasswordAuthenticationMethod(userName, password),
                                        new PrivateKeyAuthenticationMethod(hostKey));
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    _logger.LogInformation($"Connected to {hostAddress}.");

                    foreach (var sftpPath in downloadedfileNames)
                    {
                        client.DeleteFile(sftpPath);
                    }

                    client.Disconnect();
                }
                catch (SshAuthenticationException sae)
                {
                    _logger.LogError($"Not able to login to {hostAddress} because {sae.Message}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Not able to delete data from Edi source {hostAddress} for {ex.Message}.");
                }
            }
        }
    }
}