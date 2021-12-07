using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("sales_return")]
    public class SalesReturn : EntityBase
    {
        [Key]
        [Column("sales_return_id")]
        public int SalesReturnId { get; set; }

        [Column("sales_id")]
        public int SalesId { get; set; }

        [Column("product_id")]
        public int? ProductId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("upc_code")]
        public string UpcCode { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public double Price { get; set; }

        [Column("return_amount")]
        public double? ReturnAmount { get; set; }

        [StringLength(150)]
        [Column("ref_prescription_id")]
        public string RefPrescriptionId { get; set; }

        [StringLength(50)]
        [Column("item_type")]
        public string ItemType { get; set; }

        [StringLength(50)]
        [Column("return_type")]
        public string ReturnType { get; set; }

        [StringLength(20)]
        [Column("invoice_no")]
        public string InvoiceNo { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}