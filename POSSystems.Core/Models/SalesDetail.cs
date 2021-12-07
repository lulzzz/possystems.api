using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("sales_detail")]
    public class SalesDetail : EntityBase
    {
        [Key]
        [Column("sales_detail_id")]
        public int SalesDetailId { get; set; }

        [Column("sales_id")]
        public int? SalesId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [StringLength(100)]
        [Column("upc_code")]
        public string UpcCode { get; set; }

        [StringLength(200)]
        [Column("description")]
        public string Description { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("price")]
        public double? Price { get; set; }

        [Column("unit_price_after_tax")]
        public double? UnitPriceAfterTax { get; set; }

        [StringLength(50)]
        [Column("sales_product_type")]
        public string SalesProductType { get; set; }

        [StringLength(150)]
        [Column("ref_prescription_id")]
        public string RefPrescriptionId { get; set; }

        [Column("item_type")]
        public string ItemType { get; set; }

        [Column("is_fsa")]
        public bool? IsFsa { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("SalesId")]
        public virtual SalesMaster SalesMaster { get; set; }

        [Column("discount_item_percentage")]
        public double? DiscountItemPercentage { get; set; }

        [Column("item_total_discount")]
        public double? ItemTotalDiscount { get; set; }

        [Column("supplier_id")]
        public int? SupplierId { get; set; }

        [Column("process_time")]
        public DateTime? ProcessTime { get; set; }

        [Column("dispensing_date")]
        public DateTime? DispensingDate { get; set; }
    }
}