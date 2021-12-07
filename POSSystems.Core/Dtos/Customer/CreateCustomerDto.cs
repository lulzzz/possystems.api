using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.Customer
{
    public class CreateCustomerDto
    {
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

        public int? ConfigRedeemThresholdPoint { get; set; }

        public int? ConfigInitialPointRewarded { get; set; }

        public int? ConfigDollarPointConversionRatio { get; set; }

        public int? ConfigPointDollarConversionRatio { get; set; }
    }
}