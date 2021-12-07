using Humanizer;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class BatchFileRepository : Repository<BatchFile>, IBatchFileRepository
    {
        public BatchFileRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public BatchFile GetDownloadedBatchFile(string filename)
        {
            return Context.Set<BatchFile>().Where(bf => bf.FileName == filename && bf.Status == FileStatus.Downloaded.Humanize()).OrderByDescending(bf => bf.FileId).FirstOrDefault();
        }
    }
}