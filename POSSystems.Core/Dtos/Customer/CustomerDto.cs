namespace POSSystems.Core.Dtos.Customer
{
    public class CustomerDto : DtoBase
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string LoyaltyCardNumber { get; set; }

        public int? LoyaltyPointEarned { get; set; }

        public int? RedeemThresholdPoint { get; set; }

        public int? InitialPointRewarded { get; set; }

        public double? DollarAmountSpend { get; set; }

        public int? DollarPointConversionRatio { get; set; }

        public int? PointDollarConversionRatio { get; set; }

        public double RedeemableAmount { get; set; } = 0;
    }
}