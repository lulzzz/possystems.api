using System;
using System.ComponentModel.DataAnnotations;

namespace POSSystems.Core.Dtos.PurchaseMaster
{
    public class CreatePurchaseMasterDto
    {
        public int SupplierId { get; set; }

        [MaxLength(50)]
        public string PayMethod { get; set; }

        public DateTime? PurchaseDate { get; set; }

        public double? GrandTotal { get; set; }

        public double? Payment { get; set; }

        public double? Due { get; set; }

        public string DeliveryStatus { get; set; }

        public string PurchaseMethod { get; set; }
    }
}