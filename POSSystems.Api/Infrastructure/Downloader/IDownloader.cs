namespace POSSystems.Web.Infrastructure.Downloader
{
    public interface IDownloader
    {
        string[] Download(string hostAddress, string userName, string password, int port,
            string hostKey, string downloadPath, string wildcardFileName, string outputPath, string subOutputPath, bool deleteFile = false);

        void DeleteFiles(string[] downloadedfileNames, string hostAddress, string userName, string password,
            int port, string hostKey, string downloadPath);
    }
}