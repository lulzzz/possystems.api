using AutoMapper;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Collections.Generic;

namespace POSSystems.Persistence.Repositories
{
    public class EligibleProductRepository : Repository<EligibleProduct>
    , IEligibleProductRepository
    {
        public EligibleProductRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public IMapper RepMapper { get; set; }

        public bool Exists(string upc)
        {
            var eligibleProduct = Context.Set<EligibleProduct>().Find(upc);
            if (eligibleProduct != null)
            {
                var ind = eligibleProduct.ChangeIndicator;
                if (ind == "S" || ind == "A" || string.IsNullOrEmpty(ind))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public EligibleProduct Get(string upc)
        {
            return Context.Set<EligibleProduct>().Find(upc);
        }

        public void Merge(IEnumerable<EligibleProduct> eligibleProducts)
        {
            eligibleProducts = eligibleProducts ?? throw new ArgumentNullException(nameof(eligibleProducts));

            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var eligibleProduct in eligibleProducts)
                    {
                        var ep = Get(eligibleProduct.UPC);
                        if (ep == null)
                        {
                            Add(eligibleProduct);
                        }
                        else
                        {
                            RepMapper.Map(eligibleProduct, ep);
                        }
                    }

                    Context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}