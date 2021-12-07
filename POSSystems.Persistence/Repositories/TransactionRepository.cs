using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class TransactionRepository : Repository<Transaction>
    , ITransactionRepository
    {
        public TransactionRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}