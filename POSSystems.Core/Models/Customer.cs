using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("customer")]
    public class Customer : EntityBase
    {
        [Key]
        [Column("id")]
        public int CustomerID { get; set; }

        [Required]
        [StringLength(100)]
        [Column("name")]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(50)]
        [Column("phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(100)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("loyalty_card_number")]
        public string LoyaltyCardNumber { get; set; }

        [Column("loyalty_point_earned")]
        public int? LoyaltyPointEarned { get; set; }

        [Column("redeem_threshold_point")]
        public int? RedeemThresholdPoint { get; set; }

        [Column("initial_point_rewarded")]
        public int? InitialPointRewarded { get; set; }

        [Column("dollar_amount_spend")]
        public double? DollarAmountSpend { get; set; }

        [Column("dollar_point_conversion_ratio")]
        public int? DollarPointConversionRatio { get; set; }

        [Column("point_dollar_conversion_ratio")]
        public int? PointDollarConversionRatio { get; set; }
    }
}