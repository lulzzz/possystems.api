using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("product")]
    public class Product : EntityBase
    {
        [Key]
        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("measurement_id")]
        public int MeasurementId { get; set; }

        [Column("purchase_price")]
        public double? PurchasePrice { get; set; }

        [Column("sales_price")]
        public double SalesPrice { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [Column("tax_ind")]
        public bool TaxInd { get; set; }

        [ForeignKey("MeasurementId")]
        public virtual MeasurementUnit MeasurementUnit { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        [Required]
        [StringLength(50)]
        [Column("upc_code")]
        public string UpcCode { get; set; }

        [Required]
        [StringLength(50)]
        [Column("upc_scan_code")]
        public string UpcScanCode { get; set; }

        //[Column("batch_id")]
        //public int? BatchId { get; set; }

        [Column("manufacture_date")]
        public DateTime? ManufactureDate { get; set; }

        [Column("expire_date")]
        public DateTime? ExpireDate { get; set; }

        [Column("package_size")]
        public int? PackageSize { get; set; }

        [Column("strength")]
        public string Strength { get; set; }

        [Column("itemno")]
        public string ItemNo { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Required]
        [StringLength(150)]
        [Column("product_name")]
        public string ProductName { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("min_stock")]
        public int? MinStock { get; set; }

        [Column("max_stock")]
        public int? MaxStock { get; set; }

        [Column("reorder_level")]
        public int? ReorderLevel { get; set; }

        [ForeignKey("CategoryId")]
        public virtual ProductCategory Category { get; set; }

        [Column("catalog_product_id")]
        public string CatalogProductId { get; set; }

        [Column("product_pricerange_id")]
        public int? ProductPriceRangeId { get; set; }

        [ForeignKey("ProductPriceRangeId")]
        public virtual ProductPriceRange ProductPriceRange { get; set; }

        [NotMapped]
        public bool? TaxIndicator
        {
            get
            {
                bool? tempTaxInd = TaxInd;
                if (!TaxInd)
                    tempTaxInd = Category?.TaxInd;

                return tempTaxInd;
            }
        }

        [NotMapped]
        public string CategoryName
        {
            get
            {
                if (Category != null)
                    return Category.CategoryName;

                return string.Empty;
            }
        }

        [Column("manufacturer_id")]
        public int? ManufacturerId { get; set; }

        [ForeignKey("ManufacturerId")]
        public virtual Manufacturer Manufacturer { get; set; }
    }
}