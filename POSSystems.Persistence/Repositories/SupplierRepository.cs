using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class SupplierRepository : Repository<Supplier>
    , ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}