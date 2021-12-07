using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface IPriceRangeRepository : IRepository<PriceRange>
    {
        double SelectPrice(Product product, PriceCatalog priceCatalog);

        double GetCalculatedPrice(Product product, double salesPrice, double purchasePrice);
    }
}