using Humanizer;
using Microsoft.EntityFrameworkCore;
using POSSystems.Core;
using POSSystems.Core.Dtos.Sales;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class SalesMasterRepository : Repository<SalesMaster>
    , ISalesMasterRepository
    {
        public SalesMasterRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public void Add(SalesMaster salesMaster, Transaction transaction, List<PosItemDto> productsList, List<RxItemDto> rxList, double taxPercentage)
        {
            using var dbTransaction = Context.Database.BeginTransaction();
            try
            {
                if (GetByInvoiceNo(salesMaster.InvoiceNo) == null)
                {
                    Context.ChangeTracker.AutoDetectChangesEnabled = false;
                    Context.Set<SalesMaster>().Add(salesMaster);
                    Context.SaveChanges();

                    var salesDetails = new List<SalesDetail>();
                    if (productsList != null && productsList.Any())
                    {
                        var productIds = productsList.Select(d => d.Id);
                        var products = Context.Set<Product>().Where(s => productIds.Contains(s.ProductId));
                        foreach (var product in products)
                        {
                            var selectedItem = productsList.SingleOrDefault(r => r.Id == product.ProductId);
                            product.Quantity -= selectedItem.Quantity;

                            var consideredSalesPrice = selectedItem.OverriddenPrice ?? product.SalesPrice;

                            var unitPriceAfterDiscount = selectedItem.DiscountItemPercentage.HasValue ? consideredSalesPrice - ((consideredSalesPrice * selectedItem.DiscountItemPercentage.Value) / 100) : consideredSalesPrice;

                            var unitPriceAfterTax = product.TaxInd ? unitPriceAfterDiscount + ((unitPriceAfterDiscount * taxPercentage) / 100) : unitPriceAfterDiscount;

                            var salesDetail = new SalesDetail
                            {
                                Description = product.ProductName,
                                Quantity = selectedItem.Quantity,
                                ProductId = product.ProductId,
                                Price = product.SalesPrice,
                                SalesId = salesMaster.SalesId,
                                UpcCode = product.UpcScanCode,
                                UnitPriceAfterTax = Math.Round(unitPriceAfterTax, 2, MidpointRounding.AwayFromZero),
                                ItemType = ItemType.PI.Humanize(),
                                IsFsa = selectedItem.IsFsa,
                                DiscountItemPercentage = selectedItem.DiscountItemPercentage,
                                ItemTotalDiscount = selectedItem.TotalItemDiscount,
                                SupplierId = product.SupplierId
                            };

                            salesDetails.Add(salesDetail);
                        }
                    }

                    if (rxList != null && rxList.Any())
                    {
                        foreach (var rxItem in rxList)
                        {
                            var salesDetail = new SalesDetail
                            {
                                Description = rxItem.PrescribedDrug,
                                Quantity = Convert.ToInt32(Math.Round(rxItem.DispensingQuantity.Value)),
                                SalesId = salesMaster.SalesId,
                                UnitPriceAfterTax = rxItem.Copay.HasValue ? Math.Round((rxItem.Copay.Value + rxItem.TaxAmount), 2, MidpointRounding.AwayFromZero) : default(double?),
                                RefPrescriptionId = rxItem.RxNo,
                                UpcCode = rxItem.RxNo.PadLeft(9, '0'),
                                ItemType = ItemType.RX.Humanize(),
                                IsFsa = rxItem.IsFSA ?? false,
                                ProcessTime = rxItem.ProcessTime,
                                DispensingDate = rxItem.DispensingDate
                            };

                            salesDetails.Add(salesDetail);
                        }
                    }

                    Context.Set<SalesDetail>().AddRange(salesDetails);
                }

                transaction.SalesId = salesMaster.SalesId;
                Context.Set<Transaction>().Add(transaction);

                Context.ChangeTracker.DetectChanges();
                Context.SaveChanges();
                dbTransaction.Commit();
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }

            Context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public SalesMaster GetByInvoiceNo(string invoiceNo, bool withDetail = false)
        {
            if (withDetail)
                return Context.Set<SalesMaster>()
                    .Include(s => s.SalesDetails)
                    .Include(s => s.Transactions)
                    .SingleOrDefault(i => i.InvoiceNo == invoiceNo);
            else
                return Context.Set<SalesMaster>().SingleOrDefault(i => i.InvoiceNo == invoiceNo);
        }

        public int? GetLastInvoiceNo()
        {
            int? invoiceNo = 0;
            //using (var connection = Context.Database.GetDbConnection())
            //{
            //    connection.Open();

            //    using (var command = connection.CreateCommand())
            //    {
            //        command.CommandText = "SELECT IDENT_CURRENT('SALES_MASTER')";
            //        var result = command.ExecuteScalar().ToString();
            //        invoiceNo = int.Parse(result);
            //    }
            //}

            invoiceNo = Context.Set<SalesMaster>()
                .OrderByDescending(s => Convert.ToInt32(s.InvoiceNo))
                .Select(r => Convert.ToInt32(r.InvoiceNo))
                .FirstOrDefault();

            return invoiceNo;
        }
    }
}