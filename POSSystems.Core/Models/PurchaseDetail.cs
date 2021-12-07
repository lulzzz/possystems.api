using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("purchase_detail")]
    public class PurchaseDetail : EntityBase
    {
        [Key]
        [Required]
        [Column("purchase_detail_id")]
        public int PurchaseDetailId { get; set; }

        [Required]
        [Column("purchase_id")]
        public int PurchaseId { get; set; }

        [StringLength(50)]
        [Column("upc_scan_code")]
        public string UpcScanCode { get; set; }

        [Required]
        [Column("product_id")]
        public int ProductId { get; set; }

        [StringLength(200)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public double? Price { get; set; }

        [Column("delivery_status")]
        public string DeliveryStatus { get; set; }

        [Column("delivered_quantity")]
        public int? DeliveredQuantity { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        [ForeignKey("PurchaseId")]
        public virtual PurchaseMaster Purchase { get; set; }
    }
}