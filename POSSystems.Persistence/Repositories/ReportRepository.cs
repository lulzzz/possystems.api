using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using POSSystems.Core.Dtos.Product;
using POSSystems.Core.Dtos.Report;
using POSSystems.Core.Dtos.SalesMaster;
using POSSystems.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace POSSystems.Persistence.Repositories
{
    public class ReportRepository : IReportRepository
    {
        protected readonly ApplicationDbContext Context;

        public ReportRepository(ApplicationDbContext context) => Context = context;

        public IConfigurationRoot Configuration { get; }

        public IEnumerable<SalesMasterDto> GetAllSales(DateTime startDate, DateTime endDate, int? terminalId = null, string user = null, int? supplierId = null)
        {
            using IDbConnection dbConnection = Context.Database.GetDbConnection();
            var dbParams = new DynamicParameters();
            dbParams.Add("startDate", startDate);
            dbParams.Add("endDate", endDate);

            string query = @"SELECT
                    sm.[invoice_no] as InvoiceNo,
                    sm.sales_date as SalesDate,
                    sm.grand_total as GrandTotal,
                    sm.sales_tax as SalesTax,
                    sm.discount_total as TotalDiscount,
                    (
		                select sum(IIF(tr.transaction_type = 'S', tr.amount, 0)) from [transaction] as tr where convert(date, tr.created_date) = convert(date, @startDate) and tr.sales_id = sm.sales_id
	                ) as Payment,
                    sm.due,
                    (
	                    select sum(sr.return_amount) from sales_return as sr where sr.created_date BETWEEN @startDate and @endDate and sr.sales_id = sm.sales_id
                    ) as ReturnAmount
                    FROM
                    sales_master as sm
                    WHERE sm.sales_date BETWEEN @startDate AND @endDate";

            if (terminalId != null)
            {
                query += " and sm.terminal_id = @terminalId";
                dbParams.Add("terminalId", terminalId);
            }
            else if (user != null && user != "All")
            {
                query += " and lower(sm.created_by) = @user";
                dbParams.Add("user", user.ToLowerInvariant());
            }

            query += " GROUP BY sm.invoice_no, sm.grand_total, sm.sales_tax, sm.due, sm.discount_total, sm.sales_id, sm.sales_date, sm.payment";

            var product = dbConnection.Query<SalesMasterDto>(query, dbParams);

            return product;
        }

        public IEnumerable<DetailedSalesDto> GetDetailedSalesForRx(DateTime startDate, DateTime endDate, int? terminalId = null, string user = null)
        {
            using IDbConnection dbConnection = Context.Database.GetDbConnection();
            var dbParams = new DynamicParameters();
            dbParams.Add("startDate", startDate);
            dbParams.Add("endDate", endDate);
            string query = @"SELECT
                sales_master.[sales_date]  as SalesDate,
                sales_detail.[description] as ProductName,
                null as UPCCode,
                SUM(sales_detail.[quantity]) as Quantity,
                null as Price,
                null as Discount,
                SUM(sales_detail.[unit_price_after_tax]) as SalesPrice,
                null as SupplierName
                FROM
                sales_master JOIN sales_detail ON sales_master.[sales_id] = sales_detail.[sales_id]
                WHERE sales_detail.[description] is not null
				AND sales_master.[sales_date] BETWEEN @startDate AND @endDate";

            if (terminalId != null)
            {
                query += " and sales_master.terminal_id=@terminalId";
                dbParams.Add("terminalId", terminalId);
            }
            else if (user != null && user != "All")
            {
                query += " and lower(sales_master.created_by) = @user";
                dbParams.Add("user", user.ToLowerInvariant());
            }

            query += " GROUP BY sales_master.[sales_date], sales_detail.[description]";
            query += " ORDER BY sales_detail.[description], sales_master.[sales_date]";

            var detailedSales = dbConnection.Query<DetailedSalesDto>(query, dbParams);

            return detailedSales;
        }

        public IEnumerable<TimesheetDto> GetTimesheet(DateTime startTime, DateTime endTime, int? userId)
        {
            return Context.Sessions
                .Include(s => s.User)
                .Where(s =>
                (
                    (
                        (s.StartTime >= startTime && s.StartTime <= endTime)
                        || (s.EndTime >= startTime && s.EndTime <= endTime)
                        || (s.StartTime < startTime && s.EndTime > endTime)
                    )
                    && (s.StartTime < s.EndTime)
                )
                && (userId == null || s.UserId == userId))
                .Select(s => new TimesheetDto
                {
                    UserId = s.UserId,
                    UserName = s.User.UserName,
                    StartTime = (s.StartTime < startTime && s.EndTime > endTime) ? startTime : s.StartTime,
                    EndTime = (s.StartTime < startTime && s.EndTime > endTime) ? endTime : s.EndTime
                });
        }

        public IEnumerable<DetailedSalesDto> GetDetailedSales(DateTime startDate, DateTime endDate, int? terminalId = null, string user = null, int? supplierId = null)
        {
            using IDbConnection dbConnection = Context.Database.GetDbConnection();
            var dbParams = new DynamicParameters();
            dbParams.Add("startDate", startDate);
            dbParams.Add("endDate", endDate);
            string query = @"SELECT
				sales_master.[sales_date]  as SalesDate,
				product.[product_name] as ProductName,
				product.[upc_scan_code] as UPCCode,
				SUM(sales_detail.[quantity]) as Quantity,
				product.[sales_price] as Price,
				SUM(sales_detail.[item_total_discount]) as Discount,
				product.[sales_price] * SUM(sales_detail.[quantity]) - SUM(sales_detail.[item_total_discount]) as SalesPrice,
				supplier.[supplier_name] as SupplierName
                FROM
                sales_master JOIN sales_detail ON sales_master.[sales_id] = sales_detail.[sales_id]
				LEFT JOIN product ON sales_detail.[product_id] = product.[product_id]
				LEFT JOIN supplier ON supplier.[supplier_id] = product.[supplier_id]
                WHERE product.[upc_scan_code] != ''
				AND sales_master.[sales_date] BETWEEN @startDate AND @endDate";

            if (terminalId != null)
            {
                query += " and sales_master.terminal_id=@terminalId";
                dbParams.Add("terminalId", terminalId);
            }
            else if (user != null && user != "All")
            {
                query += " and lower(sales_master.created_by) = @user";
                dbParams.Add("user", user.ToLowerInvariant());
            }
            else if (supplierId != null)
            {
                query += " and supplier.supplier_id=@supplierId";
                dbParams.Add("supplierId", supplierId);
            }

            query += " GROUP BY sales_master.[sales_date], product.[product_name], product.[upc_scan_code], product.[sales_price], supplier.[supplier_name]";
            query += " ORDER BY supplier.[supplier_name], product.[product_name]";

            var detailedSales = dbConnection.Query<DetailedSalesDto>(query, dbParams);

            return detailedSales;
        }

        public IEnumerable<ProductStockDto> GetProductsStock(string upcscancode, string productname, int? categoryId = null, int? supplierId = null)
        {
            using IDbConnection dbConnection = Context.Database.GetDbConnection();
            var dbParams = new DynamicParameters();

            string query = @"SELECT
					product.[upc_scan_code] as UPCCode,
                    product.[product_name] as ProductName,
					product_category.[category_name] as CategoryName,
                    product.[quantity] as Quantity,
                    product.[purchase_price] as PurchasePrice,
                    product.[sales_price] as SalesPrice
					FROM product
					join product_category on product.category_id = product_category.category_id
					left outer JOIN supplier ON supplier.supplier_id = product.supplier_id ";

            if (!string.IsNullOrEmpty(upcscancode))
            {
                query += " where product.upc_scan_code=@upcscancode";
                dbParams.Add("upcscancode", upcscancode);
            }
            else if (!string.IsNullOrEmpty(productname))
            {
                query += " where product.product_name Like '" + productname + "%'";
                dbParams.Add("productname", productname);
            }
            else if (categoryId != null)
            {
                query += " where product_category.category_id=@categoryId";
                dbParams.Add("categoryId", categoryId);
            }
            else if (supplierId != null)
            {
                query += " where supplier.supplier_id=@supplierId";
                dbParams.Add("supplierId", supplierId);
            }

            query += " order by product.product_name, product_category.category_name";

            var products = dbConnection.Query<ProductStockDto>(query, dbParams);

            return products;
        }

        public SalesEndDto GetSalesEnd(DateTime theDate, string user = null)
        {
            using IDbConnection dbConnection = Context.Database.GetDbConnection();
            var dbParams = new DynamicParameters();
            dbParams.Add("theDate", theDate);
            string query = @"SELECT
                    sum(sm.grand_total) as Total,
                    sum(sm.sales_tax) as TotalTax,
                    sum(sm.points_earned) as TotalPointsEarned,
                    sum(sm.points_redeemed) as TotalPointsRedeemed,
                    sum(sm.discount_total) as TotalDiscount,
                    count(sm.sales_id) as TotalCustomer
                    FROM
                    sales_master as sm
                    where sm.sales_date = convert(datetime, @theDate)";

            if (user != null && user != "All")
            {
                query += " and lower(sm.created_by) = @user";
                dbParams.Add("user", user.ToLowerInvariant());
            }

            var salesEndDto = dbConnection.QuerySingle<SalesEndDto>(query, dbParams);

            var dbTypeParams = new DynamicParameters();
            dbTypeParams.Add("theDate", theDate);
            string typequery = @"SELECT
                tr.transaction_type as K,
                sum(tr.amount) as V
                FROM
                [transaction] tr
                where convert(date, tr.created_date) = convert(date, @theDate)";

            if (user != null && user != "All")
            {
                typequery += " and lower(tr.created_by) = @user";
                dbTypeParams.Add("user", user.ToLowerInvariant());
            }

            typequery += " group by tr.transaction_type";

            var salesEndTypeKeyValDto = dbConnection.Query<SalesEndKeyValDto>(typequery, dbTypeParams);

            var dbPayParams = new DynamicParameters();
            dbPayParams.Add("theDate", theDate);
            string payquery = @"SELECT
                tr.payment_method as K,
                sum(tr.amount) as V
                FROM
                [transaction] tr
                where convert(date, tr.created_date) = convert(datetime, @theDate)";

            if (user != null && user != "All")
            {
                payquery += " and lower(tr.created_by) = @user";
                dbPayParams.Add("user", user.ToLowerInvariant());
            }

            payquery += " group by tr.payment_method";

            var salesEndPayKeyValDto = dbConnection.Query<SalesEndKeyValDto>(payquery, dbPayParams);

            var dbCardParams = new DynamicParameters();
            dbCardParams.Add("theDate", theDate);
            string cardquery = @"SELECT
                tr.card_type as K,
                sum(tr.amount) as V
                FROM
                [transaction] tr
                where card_type is not null and convert(date, tr.created_date) = convert(datetime, @theDate)";

            if (user != null && user != "All")
            {
                cardquery += " and lower(tr.created_by) = @user";
                dbCardParams.Add("user", user.ToLowerInvariant());
            }

            cardquery += " group by tr.card_type";

            var salesEndCardKeyValDto = dbConnection.Query<SalesEndKeyValDto>(cardquery, dbCardParams);

            var dbCategoryParams = new DynamicParameters();
            dbCategoryParams.Add("theDate", theDate);
            string categoryquery = @"SELECT pc.category_name as K, sum(sd.quantity * sd.unit_price_after_tax) as V
                  FROM [sales_detail] as sd
                  inner join [product] as p
                  on sd.product_id = p.product_id
                  inner join [product_category] as pc
                  on pc.category_id = p.category_id
                  where convert(date, sd.created_date) = convert(datetime, @theDate)";

            if (user != null && user != "All")
            {
                categoryquery += " and lower(sd.created_by) = @user";
                dbCategoryParams.Add("user", user.ToLowerInvariant());
            }

            categoryquery += " group by category_name";

            var salesEndCategoryKeyValDto = dbConnection.Query<SalesEndKeyValDto>(categoryquery, dbCategoryParams).AsList();

            var dbCategoryRxParams = new DynamicParameters();
            dbCategoryRxParams.Add("theDate", theDate);
            string categoryRxquery = @"SELECT 'RX' as K, sum(sd.unit_price_after_tax) as V
                    FROM [sales_detail] as sd
                    where convert(date, sd.created_date) = convert(datetime, @theDate)
                    and sd.item_type = 'RX'
                    ";

            if (user != null && user != "All")
            {
                categoryRxquery += " and lower(sd.created_by) = @user";
                dbCategoryRxParams.Add("user", user.ToLowerInvariant());
            }

            var salesEndCategoryRxKeyValDto = dbConnection.QuerySingle<SalesEndKeyValDto>(categoryRxquery, dbCategoryRxParams);

            foreach (var s in salesEndTypeKeyValDto)
            {
                if (s.K == "S")
                    salesEndDto.Sales = s.V;
                else if (s.K == "SR")
                    salesEndDto.Return = s.V;
            }

            foreach (var s in salesEndPayKeyValDto)
            {
                if (s.K == "Bank")
                    salesEndDto.Bank = s.V;
                else if (s.K == "Card")
                    salesEndDto.Card = s.V;
                else if (s.K == "Cash")
                    salesEndDto.Cash = s.V;
            }

            foreach (var s in salesEndCardKeyValDto)
            {
                if (s.K == "M/C")
                    salesEndDto.MasterCard = s.V;
                else if (s.K == "VISA")
                    salesEndDto.VisaCard = s.V;
                else
                    salesEndDto.Other = +s.V;
            }

            salesEndDto.CategoryPrices = salesEndCategoryKeyValDto;
            if (salesEndCategoryRxKeyValDto != null)
                salesEndDto.CategoryPrices.Add(salesEndCategoryRxKeyValDto);

            return salesEndDto;
        }
    }
}