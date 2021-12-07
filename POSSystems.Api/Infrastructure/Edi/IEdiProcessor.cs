namespace POSSystems.Web.Infrastructure.Edi
{
    public interface IEdiProcessor
    {
        void Process(string[] downloadedFiles, string ediPath, string inPath, string processingPath, int supplierId, string fieldSeparator);
    }
}