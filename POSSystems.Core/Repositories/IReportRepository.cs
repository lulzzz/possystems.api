using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Dtos.Report;
using POSSystems.Core.Dtos.SalesMaster;
using System;
using System.Collections.Generic;

namespace POSSystems.Core.Repositories
{
    public interface IReportRepository
    {
        IEnumerable<SalesMasterDto> GetAllSales(DateTime startDate, DateTime endDate, int? terminalId = null, string userId = null, int? supplierId = null);

        SalesEndDto GetSalesEnd(DateTime date, string userId = null);

        IEnumerable<ProductStockDto> GetProductsStock(string productname, string upcscancode, int? categoryId = null, int? supplierId = null);

        IEnumerable<DetailedSalesDto> GetDetailedSales(DateTime startDate, DateTime endDate, int? terminalId = null, string userId = null, int? supplierId = null);

        IEnumerable<DetailedSalesDto> GetDetailedSalesForRx(DateTime startDate, DateTime endDate, int? terminalId = null, string user = null);

        IEnumerable<TimesheetDto> GetTimesheet(DateTime startDate, DateTime endDate, int? userId = null);
    }
}