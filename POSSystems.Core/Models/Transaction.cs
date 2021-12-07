using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("transaction")]
    public class Transaction : EntityBase
    {
        [Key]
        [Required]
        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("sales_id")]
        public int? SalesId { get; set; }

        [Column("purchase_id")]
        public int? PurchaseId { get; set; }

        [StringLength(50)]
        [Column("payment_method")]
        public string PayMethod { get; set; }

        [StringLength(50)]
        [Column("masked_account")]
        public string MaskedAcct { get; set; }

        [Column("amount")]
        public double Amount { get; set; }

        [Column("back")]
        public double? Back { get; set; }

        [Column("transaction_type")]
        public string TransactionType { get; set; }

        [Column("token")]
        public string Token { get; set; }

        [Column("card_type")]
        public string CardType { get; set; }

        [StringLength(50)]
        [Column("check_no")]
        public string CheckNo { get; set; }

        [StringLength(20)]
        [Column("authcode")]
        public string AuthCode { get; set; }

        [StringLength(30)]
        [Column("acqrefdata")]
        public string AcqRefData { get; set; }

        [ForeignKey("SalesId")]
        public virtual SalesMaster Sales { get; set; }

        [ForeignKey("PurchaseId")]
        public virtual PurchaseMaster Purchase { get; set; }
    }
}