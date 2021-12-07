using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IBatchFileRepository : IRepository<BatchFile>
    {
        BatchFile GetDownloadedBatchFile(string filename);
    }
}