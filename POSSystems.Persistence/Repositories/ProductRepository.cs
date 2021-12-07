using Microsoft.EntityFrameworkCore;
using POSSystems.Core.Models;
using POSSystems.Core.Repositories;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Persistence.Repositories
{
    public class ProductRepository : Repository<Product>
    , IProductRepository
    {
        public ProductRepository(ApplicationDbContext context)
        : base(context)
        {
        }

        public IEnumerable<Product> GetAll(Expression<Func<Product, bool>> predicate)
        {
            return Context.Set<Product>().Where(predicate).AsNoTracking().Include(p => p.Category).ToList();
        }

        public Product GetOne(int id)
        {
            return Context.Set<Product>().Where(pd => pd.ProductId == id).Include(p => p.Supplier).Include(p => p.MeasurementUnit).Include(p => p.Category).SingleOrDefault();
        }

        public new IQueryable<Product> GetAllDeferred(Expression<Func<Product, bool>> predicate)
        {
            return Context.Set<Product>()
                .Where(predicate)
                .Include(p => p.Supplier)
                .Include(p => p.MeasurementUnit)
                .Include(p => p.Category)
                .Include(p => p.ProductPriceRange);
        }

        public Product GetReadOnly(Expression<Func<Product, bool>> predicate)
        {
            return Context.Set<Product>().AsNoTracking().Where(predicate).Include(p => p.MeasurementUnit).Include(p => p.Category).SingleOrDefault();
        }

        public new IQueryable<Product> GetAllDeferred()
        {
            return Context.Set<Product>().Include(p => p.Supplier).Include(p => p.MeasurementUnit).Include(p => p.Category);
        }

        public new void Add(Product product)
        {
            //var productMaster = Context.Set<ProductMaster>().Where(pm => pm.ProductId == productDetail.ProductId).Include(p => p.Category).SingleOrDefault();
            //if (productMaster != null)
            //{
            //productDetail.TaxInd = productDetail.TaxInd || productMaster.Category.TaxInd;

            Context.Set<Product>().Add(product);
            //}
        }

        public Product GetByUpc(string upcScanCode)
        {
            return Context.Set<Product>().Where(pd => pd.UpcScanCode == upcScanCode).Include(p => p.Supplier).Include(p => p.MeasurementUnit).SingleOrDefault();
        }

        public Product GetByScanUpc(string upcScanCode)
        {
            return Context.Set<Product>().Where(pd => pd.UpcScanCode == upcScanCode).SingleOrDefault();
        }

        public Product GetByRealUpc(string upcCode)
        {
            return Context.Set<Product>().Where(pd => pd.UpcCode == upcCode).SingleOrDefault();
        }

        public bool ExistsRealUpc(string upcCode)
        {
            return Context.Set<Product>().Any(pd => pd.UpcCode == upcCode);
        }

        public bool ExistsScanUpc(string upcScanCode)
        {
            return Context.Set<Product>().Any(pd => pd.UpcScanCode == upcScanCode);
        }

        public IQueryable<Product> GetReorderPendingProducts()
        {
            var reorderPendingProducts = (from p in Context.Products
                                          group p by new { p.ProductName, p.ReorderLevel, p.ProductId } into g
                                          where g.Sum(t3 => t3.Quantity) <= g.Key.ReorderLevel
                                          //orderby (g.Sum(t3 => t3.Quantity) - g.Key.ReorderLevel) descending
                                          select new Product
                                          {
                                              ProductName = g.Key.ProductName,
                                              ReorderLevel = g.Key.ReorderLevel,
                                              ProductId = g.Key.ProductId
                                          }).Take(10);

            return reorderPendingProducts;
        }

        public Product GetProductByCatalog(string catalogProductId, bool addClauses = true)
        {
            var firstqueryable = Context.Set<Product>().Where(pm => pm.CatalogProductId == catalogProductId);

            if (addClauses)
                firstqueryable = firstqueryable.Include(pm => pm.Category).Include(pm => pm.ProductPriceRange);

            return firstqueryable.SingleOrDefault();
        }

        public void MergeProducts(int fileId, int measurementId, int supplierId, bool directSalesPrice)
        {
            var commandText = "EXEC ImportRdcDrugDetails @fileId, @measurementId, @supplierId, @directSalesPrice";

            var fileParam = new SqlParameter("@fileId", System.Data.SqlDbType.Int);
            var measurementParam = new SqlParameter("@measurementId", System.Data.SqlDbType.Int);
            var supplierParam = new SqlParameter("@supplierId", System.Data.SqlDbType.Int);
            var directSalesPriceParam = new SqlParameter("@directSalesPrice", System.Data.SqlDbType.Bit);

            fileParam.Value = fileId;
            measurementParam.Value = measurementId;
            supplierParam.Value = supplierId;
            directSalesPriceParam.Value = directSalesPrice;

            Context.Database.ExecuteSqlRaw(commandText, fileParam, measurementParam, supplierParam, directSalesPriceParam);
        }

        public void MergeProducts(int fileId, int supplierId, bool directSalesPrice, string username)
        {
            var commandText = "EXEC ImportProducts @fileId, @supplierId, @directSalesPrice, @user_name";

            var fileParam = new SqlParameter("@fileId", System.Data.SqlDbType.Int);
            var supplierParam = new SqlParameter("@supplierId", System.Data.SqlDbType.Int);
            var directSalesPriceParam = new SqlParameter("@directSalesPrice", System.Data.SqlDbType.Bit);
            var usernameParam = new SqlParameter("@user_name", System.Data.SqlDbType.NVarChar, 50);

            fileParam.Value = fileId;
            supplierParam.Value = supplierId;
            directSalesPriceParam.Value = directSalesPrice;
            usernameParam.Value = username;

            Context.Database.ExecuteSqlRaw(commandText, fileParam, supplierParam, directSalesPriceParam, usernameParam);
        }
    }
}