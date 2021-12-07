using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("sales_master")]
    public class SalesMaster : EntityBase
    {
        [Key]
        [Required]
        [Column("sales_id")]
        public int SalesId { get; set; }

        [Column("customer_id")]
        public int? CustomerId { get; set; }

        [StringLength(50)]
        [Column("pay_method")]
        public string PayMethod { get; set; }

        [Column("sales_date")]
        public DateTime? SalesDate { get; set; }

        [Required]
        [Column("grand_total")]
        public double GrandTotal { get; set; }

        [Required]
        [Column("payment")]
        public double Payment { get; set; }

        public double? Due
        {
            get
            {
                return GrandTotal - Payment;
            }
        }

        [Column("discount_total")]
        public double? TotalDiscount { get; set; }

        [Column("discount_percentage")]
        public double? DiscountPercentage { get; set; }

        [Column("sales_tax")]
        public double? SalesTax { get; set; }

        [Column("fsa_total")]
        public double? FsaTotal { get; set; }

        [Column("rx_fsa_total")]
        public double? RxFsaTotal { get; set; }

        [StringLength(50)]
        [Column("driving_license_no")]
        public string DrivingLicenseNo { get; set; }

        [Required]
        [Column("invoice_no")]
        public string InvoiceNo { get; set; }

        [Column("invoice_receipt")]
        public string InvoiceReceipt { get; set; }

        [Column("payment_status")]
        public string PaymentStatus { get; set; }

        [Column("returned_amount")]
        public double? ReturnedAmount { get; set; }

        [Column("terminal_id")]
        public int? TerminalId { get; set; }

        [Column("signature_needed")]
        public bool? SignatureNeeded { get; set; }

        [Column("signature")]
        public string Signature { get; set; }

        [Column("contains_rx")]
        public bool? ContainsRx { get; set; }

        [Column("points_earned")]
        public int? PointsEarned { get; set; }

        [Column("points_redeemed")]
        public int? PointsRedeemed { get; set; }

        [Column("points_returned")]
        public int? PointsReturned { get; set; }

        [Column("point_dollar_conversion_ratio")]
        public int? PointDollarConversionRatio { get; set; }

        [Column("loyalty_card")]
        public string LoyaltyCard { get; set; }

        [Column("invoice_redeem_amount")]
        public double? InvoiceRedeemAmount { get; set; }

        public virtual PosTerminal Terminal { get; set; }

        public virtual IList<SalesDetail> SalesDetails { get; set; }

        public virtual IList<Transaction> Transactions { get; set; }
        public bool IsSignatureRequired()
        {
            return PaymentStatus.DehumanizeTo<PaymentStatus>() == Core.PaymentStatus.Complete
                && SignatureNeeded == true
                && string.IsNullOrEmpty(Signature);
        }
    }
}