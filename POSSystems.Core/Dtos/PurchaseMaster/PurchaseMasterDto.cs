using System;

namespace POSSystems.Core.Dtos.PurchaseMaster
{
    public class PurchaseMasterDto : DtoBase
    {
        public int Id { get; set; }

        public int SupplierId { get; set; }

        public string PayMethod { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public double? GrandTotal { get; set; }

        public double? Payment { get; set; }

        public double? Due { get; set; }

        public string Supplier { get; set; }

        public string DeliveryStatus { get; set; }

        public string PurchaseMethod { get; set; }
    }
}