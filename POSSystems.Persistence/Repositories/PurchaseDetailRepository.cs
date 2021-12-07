using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class PurchaseDetailRepository : Repository<PurchaseDetail>
    , IPurchaseDetailRepository
    {
        public PurchaseDetailRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<PurchaseDetail> GetAllDeferred(Expression<Func<PurchaseDetail, bool>> predicate)
        {
            return Context.Set<PurchaseDetail>().Where(predicate).Include(p => p.Product);
        }

        public new void Add(PurchaseDetail purchaseDetail)
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    var product = Context.Products.Where(p => p.ProductId == purchaseDetail.ProductId).SingleOrDefault();
                    if (purchaseDetail != null && product != null)
                    {
                        product.Quantity = product.Quantity + purchaseDetail.Quantity;
                    }
                    else
                        throw new Exception("Item not found in the inventory");

                    Context.Set<PurchaseDetail>().Add(purchaseDetail);
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