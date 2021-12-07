using POSSystems.Core.Models;
using System.Linq;

namespace POSSystems.Core.Repositories
{
    public interface IPurchaseMasterRepository : IRepository<PurchaseMaster>
    {
        IQueryable<PurchaseMaster> GetDeliveryPendingPurchases();
    }
}