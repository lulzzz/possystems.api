using Humanizer;
using POSSystems.Core;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class SalesReturnRepository : Repository<SalesReturn>
    , ISalesReturnRepository
    {
        public SalesReturnRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public new IQueryable<SalesReturn> GetAllDeferred()
        {
            return Context.Set<SalesReturn>();
        }

        public void Add(SalesMaster salesMaster, Customer customer,
            Transaction transaction, List<SalesReturn> salesReturns,
            string paymentStatus, double amountAuthorized, double loyaltyAmountReturned = 0)
        {
            using (var dbTransaction = Context.Database.BeginTransaction())
            {
                try
                {
                    if (salesMaster.PaymentStatus.DehumanizeTo<PaymentStatus>() == PaymentStatus.Complete)
                    {
                        foreach (var salesReturn in salesReturns)
                        {
                            if (salesReturn.ItemType == "PI")
                            {
                                var product = Context.Set<Product>().Where(s => s.ProductId == salesReturn.ProductId).SingleOrDefault();
                                product.Quantity = product.Quantity + salesReturn.Quantity;
                            }

                            Context.Set<SalesReturn>().Add(salesReturn);
                        }
                    }
                    Context.SaveChanges();

                    transaction.SalesId = salesMaster.SalesId;
                    Context.Set<Transaction>().Add(transaction);
                    Context.SaveChanges();

                    salesMaster.PaymentStatus = paymentStatus;
                    salesMaster.ReturnedAmount = (salesMaster.ReturnedAmount ?? 0) + amountAuthorized;
                    salesMaster.PointsReturned = Convert.ToInt32(Math.Round(loyaltyAmountReturned * (salesMaster.PointDollarConversionRatio ?? 0), MidpointRounding.AwayFromZero));
                    Context.SaveChanges();

                    if (customer != null)
                    {
                        customer.LoyaltyPointEarned += (salesMaster.PointsReturned ?? 0);
                    }

                    Context.SaveChanges();
                    dbTransaction.Commit();
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }
    }
}