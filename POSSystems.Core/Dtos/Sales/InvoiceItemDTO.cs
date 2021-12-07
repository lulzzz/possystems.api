namespace POSSystems.Core.Dtos.Sales
{
    public class InvoiceItemDTO
    {
        public int ProductId { get; set; }
        public int ProductDetailsId { get; set; }
        public string ProductName { get; set; }
        public string BatchName { get; set; }
        public string Quantity { get; set; }
        public string PurchasePrice { get; set; }
        public string SalesPrice { get; set; }
        public string TotalPrice { get; set; }
        public string ItemTotalDiscount { get; set; }
        public string OverriddenPrice { get; set; }
        public string RefPrescriptionId { get; set; }
    }
}