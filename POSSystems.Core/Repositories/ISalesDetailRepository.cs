using POSSystems.Core.Models;

namespace POSSystems.Core.Repositories
{
    public interface ISalesDetailRepository : IRepository<SalesDetail>
    {
        bool ItemExistsInInvoice(string invoiceNo, int productId, int quantity);
    }
}