using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Customer
{
    public class UpdateCustomerDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(50)]
        public string Phone { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string LoyaltyCardNumber { get; set; }

        public int? LoyaltyPointEarned { get; set; }

        public int? RedeemThresholdPoint { get; set; }

        public int? InitialPointRewarded { get; set; }

        public double? DollarAmountSpend { get; set; }

        public int? DollarPointConversionRatio { get; set; }

        public int? PointDollarConversionRatio { get; set; }
    }
}