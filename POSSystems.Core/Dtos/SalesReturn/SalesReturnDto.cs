namespace POSSystems.Core.Dtos.SalesReturn
{
    public class SalesReturnDto : DtoBase
    {
        public int Id { get; set; }

        public int SalesId { get; set; }

        public int ProductDetailId { get; set; }

        public string UpcCode { get; set; }

        public int Quantity { get; set; }

        public double? Price { get; set; }

        public double? ReturnAmount { get; set; }

        public string RefPrescriptionId { get; set; }

        public string ItemType { get; set; }

        public string ReturnType { get; set; }

        public string InvoiceNo { get; set; }

        public string Product { get; set; }
    }
}