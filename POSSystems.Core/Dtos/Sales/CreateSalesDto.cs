using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class CreateSalesDto
    {
        public string BatchId { get; set; }
        public string BatchOrRx { get; set; }
        public double PaidTotal { get; set; }
        public double Total { get; set; }
        public List<RxItemDto> RxList { get; set; }
        public List<PosItemDto> PosItems { get; set; }
        public string InvoiceNo { get; set; }
        public string PickerId { get; set; }
        public string PickerMode { get; set; }
        public string PickerName { get; set; }
        public string RelationId { get; set; }
        public string VerificationId { get; set; }
        public string VerificationIdType { get; set; }
        public string PaymentType { get; set; }
        public string CheckNo { get; set; }
        public double FSATotal { get; set; } = 0;
        public double RxFSATotal { get; set; } = 0;
        public bool UpdateVss { get; set; } = false;
        public double TaxTotal { get; set; }
        public double DiscountTotal { get; set; }
        public double? DiscountPercentage { get; set; }

        public string LoyaltyCard { get; set; }
        public bool? AddPointAndDoNotRedeem { get; set; }
        public double? PointsEarned { get; set; }
        public int PointsRedeemed { get; set; }
        public double InvoiceRedeemAmount { get; set; } = 0;
        public int? PointsGained { get; set; }

        public bool SignatureReq { get; set; }
    }
}