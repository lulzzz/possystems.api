using POSSystems.Core;
using System.IO;
using System.IO.Compression;

namespace POSSystems.Web.Infrastructure.Edi
{
    public class RetailInsightsProcessor
    {
        private readonly IUnitOfWork _unitOfWork;

        public RetailInsightsProcessor(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath)
        {
            string processedPath = Path.Combine(ediPath, processingPath);

            if (!string.IsNullOrEmpty(processingPath) && !Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            if (downloadedFiles == null)
                downloadedFiles = Directory.GetFiles(Path.Combine(ediPath, inPath));

            foreach (string file in downloadedFiles)
            {
                var path = Path.GetDirectoryName(file);
                var subPath = Path.Combine(path, Path.GetFileNameWithoutExtension(file));

                if (!Directory.Exists(subPath))
                    Directory.CreateDirectory(subPath);

                ZipFile.ExtractToDirectory(file, subPath);
                File.Delete(file);

                //***TODO***
            }
        }
    }
}