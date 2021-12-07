using System;

namespace POSSystems.Core.Dtos.PurchaseMaster
{
    public class PurchaseEdi850Model
    {
        public int OrderId { get; set; }
        public int ReorderUnits { get; set; }
        public int VendorItemNo { get; set; }
        public string UpcCode { get; set; }
        public int ProductId { get; set; }
        public int MeasurementId { get; set; }
        public string Description { get; set; }
        public double? Price { get; set; }
        public DateTime OrderConfirmationDate { get; set; }
        public string DeliveryStatus { get; set; }
        public int DeliveryQuantity { get; set; }
    }
}