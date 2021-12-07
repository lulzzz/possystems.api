using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("purchase_master")]
    public class PurchaseMaster : EntityBase
    {
        [Key]
        [Required]
        [Column("purchase_id")]
        public int PurchaseId { get; set; }

        [Column("supplier_id")]
        public int SupplierId { get; set; }

        [StringLength(50)]
        [Column("pay_method")]
        public string PayMethod { get; set; }

        [Column("purchase_date")]
        public DateTime? PurchaseDate { get; set; }

        [Column("grand_total")]
        public double? GrandTotal { get; set; }

        [Column("payment")]
        public double? Payment { get; set; }

        [Column("due")]
        public double? Due { get; set; }

        [Column("delivery_status")]
        public string DeliveryStatus { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        public virtual IList<PurchaseDetail> PurchaseDetails { get; set; }

        [StringLength(50)]
        [Column("purchase_method")]
        public string PurchaseMethod { get; set; }
    }
}