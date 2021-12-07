using Humanizer;
using Microsoft.EntityFrameworkCore;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class PurchaseReturnRepository : Repository<PurchaseReturn>
    , IPurchaseReturnRepository
    {
        public PurchaseReturnRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<PurchaseReturn> GetAllDeferred()
        {
            return Context.Set<PurchaseReturn>().Include(pr => pr.ProductDetail);
        }

        public new void Add(PurchaseReturn purchaseReturn)
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var productDetail = Context.Products.Where(p => p.UpcScanCode == purchaseReturn.UpcScanCode && p.Status == Statuses.Active.Humanize()).SingleOrDefault();

                    //purchaseDetail.BatchId = batch.BatchId;

                    if (productDetail != null)
                    {
                        var quantity = productDetail.Quantity - purchaseReturn.Quantity;
                        if (quantity > 0)
                            productDetail.Quantity = quantity;
                    }

                    Context.Set<PurchaseReturn>().Add(purchaseReturn);

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}