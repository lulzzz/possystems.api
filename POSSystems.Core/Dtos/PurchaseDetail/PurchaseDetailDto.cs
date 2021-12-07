namespace POSSystems.Core.Dtos.PurchaseDetail
{
    public class PurchaseDetailDto : DtoBase
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }

        public int ProductDetailId { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double? Price { get; set; }

        public string Product { get; set; }
    }
}