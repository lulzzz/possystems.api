using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class ProductCategoryRepository : Repository<ProductCategory>, IProductCategoryRepository
    {
        public ProductCategoryRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public bool Exists(string category)
        {
            return Context.Set<ProductCategory>().Any(pc => pc.CategoryName == category);
        }

        public ProductCategory Get(string category)
        {
            return Context.Set<ProductCategory>().Where(pc => pc.CategoryName == category).SingleOrDefault();
        }
    }
}