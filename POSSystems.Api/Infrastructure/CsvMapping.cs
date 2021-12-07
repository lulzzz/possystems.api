using CsvHelper.Configuration;
using POSSystems.Core.Dtos;
using POSSystems.Core.Dtos.Product;

namespace POSSystems.Web.Infrastructure
{
    public sealed class CsvMapping : ClassMap<EligibleProductCSVDto>
    {
        public CsvMapping()
        {
            //AutoMap();
            Map(m => m.UPC);
            Map(m => m.GTIN);
            Map(m => m.Description);
            Map(m => m.FLC);
            Map(m => m.CategoryDescription);
            Map(m => m.SubCategoryDescription);
            Map(m => m.FinestCategoryDescription);
            Map(m => m.ManufacturerName);
            Map(m => m.ChangeDate);
            Map(m => m.ChangeIndicator);
        }
    }

    public sealed class ProductCSVMapping : ClassMap<ProductCSVDto>
    {
        public ProductCSVMapping()
        {
            //AutoMap();
            Map(m => m.ITEM_NUMBER).Index(0);
            Map(m => m.CATEGORY).Index(1);
            Map(m => m.DESCRIPTION).Index(2);
            Map(m => m.UPC).Index(3);
            Map(m => m.QTY).Index(4);
            Map(m => m.COST_PRICE).Index(7);
            Map(m => m.RETAIL_PRICE).Index(10);
        }
    }

    public sealed class ProductCSVMapping2 : ClassMap<ProductCSVDto2>
    {
        public ProductCSVMapping2()
        {
            //AutoMap();
            Map(m => m.product_name).Index(0);
            Map(m => m.upc_code).Index(1);
            Map(m => m.cndcfi).Index(2);
            Map(m => m.category_description).Index(3);
            Map(m => m.form).Index(4);
            Map(m => m.strength).Index(5);
            Map(m => m.package_size).Index(6);
            Map(m => m.sales_price).Index(7);
            Map(m => m.measurement_name).Index(8);
            Map(m => m.manufacture_name).Index(9);
            Map(m => m.category_name).Index(10);
        }
    }
}