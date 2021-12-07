using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class PriceCatalogRepository : Repository<PriceCatalog>, IPriceCatalogRepository
    {
        public PriceCatalogRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}