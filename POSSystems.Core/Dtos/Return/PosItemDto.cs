namespace POSSystems.Core.Dtos.Return
{
    public class PosItemDto
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public double UnitPriceAfterTax { get; set; }
        public string ItemType { get; set; }
    }
}