namespace POSSystems.Core.Dtos.SalesDetail
{
    public class SalesDetailDto : DtoBase
    {
        public int Id { get; set; }

        public int SalesId { get; set; }

        public int? ProductId { get; set; }

        public string UpcCode { get; set; }

        public string Description { get; set; }

        public int? Quantity { get; set; }

        public double? Price { get; set; }

        public double? UnitPriceAfterTax { get; set; }

        public string SalesProductType { get; set; }

        public string RefPrescriptionId { get; set; }

        public string Product { get; set; }

        public string ItemType { get; set; }

        public bool? IsFsa { get; set; }

        public double? DiscountItemPercentage { get; set; }

        public double? ItemTotalDiscount { get; set; }
    }
}