using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("price_catalog")]
    public class PriceCatalog
    {
        [Key]
        [Column("price_catalog_id")]
        public int PriceCatalogId { get; set; }

        [Column("file_id")]
        public int FileId { get; set; }

        [Column("product_qualifier")]
        public string ProductQualifier { get; set; }

        [Column("upc_code")]
        public string UPCCode { get; set; }

        [Column("scan_code")]
        public string ScanCode { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("category_description")]
        public string CategoryDescription { get; set; }

        [Column("product_name")]
        public string ProductName { get; set; }

        [Column("product_description")]
        public string ProductDescription { get; set; }

        [Column("form")]
        public string Form { get; set; }

        [Column("strength")]
        public string Strength { get; set; }

        [Column("unit")]
        public string Unit { get; set; }

        [Column("package_size")]
        public string PackageSize { get; set; }

        [Column("purchase_price")]
        public string PurchasePrice { get; set; }

        [Column("sales_price")]
        public string SalesPrice { get; set; }

        [Column("manufacturer")]
        public string Manufacturer { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("product_id")]
        public string ProductId { get; set; }

        [Column("error")]
        public string Error { get; set; }

        [Column("imported")]
        public bool Imported { get; set; }

        [Column("itemno")]
        public string ItemNo { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }
    }
}