using POSSystems.Core.Models;
using POSSystems.Core.Repositories;

namespace POSSystems.Persistence.Repositories
{
    public class ProductPriceRangeRepository : Repository<ProductPriceRange>
    , IProductPriceRangeRepository
    {
        public ProductPriceRangeRepository(ApplicationDbContext context)
        : base(context)
        {
        }
    }
}