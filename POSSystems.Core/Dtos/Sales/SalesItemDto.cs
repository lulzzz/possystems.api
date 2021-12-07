namespace POSSystems.Core.Dtos.Sales
{
    public class SalesItemDto
    {
        public bool IsRx { get; set; }
        public RxBatchDto RxBatchDto { get; set; }
        public ProductDto ProductDto { get; set; }
    }
}