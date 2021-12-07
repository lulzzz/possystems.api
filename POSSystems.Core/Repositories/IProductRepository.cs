using POSSystems.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace POSSystems.Core.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Product GetOne(int id);

        new IQueryable<Product> GetAllDeferred(Expression<Func<Product, bool>> predicate);

        new void Add(Product productDetail);

        Product GetByUpc(string upcCode);

        Product GetByRealUpc(string upcCode);

        Product GetByScanUpc(string upcScanCode);

        bool ExistsRealUpc(string upcCode);

        bool ExistsScanUpc(string upcScanCode);

        IEnumerable<Product> GetAll(Expression<Func<Product, bool>> predicate);

        Product GetReadOnly(Expression<Func<Product, bool>> predicate);

        IQueryable<Product> GetReorderPendingProducts();

        Product GetProductByCatalog(string catalogProductId, bool addClauses = true);

        void MergeProducts(int fileId, int supplierId, bool directSalesPrice, string username);

        void MergeProducts(int fileId, int measurementId, int supplierId, bool directSalesPrice);
    }
}