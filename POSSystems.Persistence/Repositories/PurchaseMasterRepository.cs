using Humanizer;
using Microsoft.EntityFrameworkCore;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class PurchaseMasterRepository : Repository<PurchaseMaster>
    , IPurchaseMasterRepository
    {
        public PurchaseMasterRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<PurchaseMaster> GetAllDeferred(Expression<Func<PurchaseMaster, bool>> predicate)
        {
            return Context.Set<PurchaseMaster>().Include(p => p.Supplier).Where(predicate);
        }

        public IQueryable<PurchaseMaster> GetDeliveryPendingPurchases()
        {
            return Context.Set<PurchaseMaster>().Where(pm => pm.DeliveryStatus == DeliveryStatus.Pending.Humanize())
                .Include(p => p.PurchaseDetails).ThenInclude(d => d.Product);
        }
    }
}