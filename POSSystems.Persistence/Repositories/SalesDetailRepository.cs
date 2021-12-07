using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class SalesDetailRepository : Repository<SalesDetail>
    , ISalesDetailRepository
    {
        public SalesDetailRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<SalesDetail> GetAllDeferred(Expression<Func<SalesDetail, bool>> predicate)
        {
            return Context.Set<SalesDetail>().Include(sd => sd.Product).Where(predicate);
        }

        public bool ItemExistsInInvoice(string invoiceNo, int productDetailId, int quantity)
        {
            bool valid = false;
            var item = Context.Set<SalesDetail>().Include(d => d.SalesMaster).Where(d => d.ProductId == productDetailId && d.SalesMaster.InvoiceNo == invoiceNo).SingleOrDefault();
            if (item != null && item.Quantity >= quantity)
                valid = true;

            return valid;
        }
    }
}