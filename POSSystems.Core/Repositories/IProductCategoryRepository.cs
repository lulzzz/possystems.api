using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IProductCategoryRepository : IRepository<ProductCategory>
    {
        ProductCategory Get(string category);

        bool Exists(string category);
    }
}