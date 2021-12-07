namespace POSSystems.Core.Dtos.PriceRange
{
    public class PriceRangeDto : DtoBase
    {
        public int Id { get; set; }

        public int ProductPriceRangeId { get; set; }

        public double MinRange { get; set; }

        public double MaxRange { get; set; }

        public double Markup { get; set; }

        public string TableName { get; set; }
    }
}