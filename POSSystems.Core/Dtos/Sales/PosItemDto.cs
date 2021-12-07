namespace POSSystems.Core.Dtos.Sales
{
    public class PosItemDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public double UnitPriceAfterTax { get; set; }
        public bool IsFsa { get; set; }
        public double? DiscountItemPercentage { get; set; }
        public double? TotalItemDiscount { get; set; }
        public double AfterDiscountPrice { get; set; }
        public double? OverriddenPrice { get; set; }
    }
}