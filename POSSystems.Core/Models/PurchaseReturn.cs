using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("purchase_return")]
    public class PurchaseReturn : EntityBase
    {
        [Key]
        [Required]
        [Column("purchase_return_id")]
        public int PurchaseReturnId { get; set; }

        [Required]
        [Column("purchase_id")]
        public int PurchaseId { get; set; }

        [StringLength(50)]
        [Column("upc_scan_code")]
        public string UpcScanCode { get; set; }

        [StringLength(200)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public double Price { get; set; }

        [Column("return_amount")]
        public double ReturnAmount { get; set; }

        [Required]
        [StringLength(50)]
        [Column("return_type")]
        public string ReturnType { get; set; }

        [Column("reason_id")]
        public int? ReasonId { get; set; }

        [Required]
        [Column("product_detail_id")]
        public int ProductDetailId { get; set; }

        [ForeignKey("ProductDetailId")]
        public virtual Product ProductDetail { get; set; }
    }
}