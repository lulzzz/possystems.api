using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSystems.Core.Models
{
    [Table("discount_coupon")]
    public class DiscountCoupon : EntityBase
    {
        [Key]
        [Column("Coupon_id")]
        public Int32? CouponId { get; set; }

        [Column("category_id")]
        public Int32? CategoryId { get; set; }

        [StringLength(100)]
        [Column("coupon_heading")]
        public string CouponHeading { get; set; }

        [StringLength(50)]
        [Column("coupon_type")]
        public string CouponType { get; set; }

        [Required]
        [StringLength(200)]
        [Column("description")]
        public string Description { get; set; }

        [StringLength(1)]
        [Column("isActivate")]
        public string IsActivate { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        [Column("Terms_and_Condition")]
        public string TermsAndCondition { get; set; }
    }
}