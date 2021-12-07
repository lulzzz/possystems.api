namespace POSSystems.Core.Dtos
{
    public class GetProductPriceRangeDto : DtoBase
    {
        public int Id { get; set; }

        public string TableName { get; set; }

        public string CostPreference { get; set; }
    }
}