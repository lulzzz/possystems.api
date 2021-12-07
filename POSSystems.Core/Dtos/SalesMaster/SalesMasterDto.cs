using POSSystems.Core.Dtos.PosTerminal;
using POSSystems.Core.Dtos.SalesDetail;
using POSSystems.Core.Dtos.Transaction;
using System;
using System.Collections.Generic;

namespace POSSystems.Core.Dtos.SalesMaster
{
    public class SalesMasterDto : DtoBase
    {
        public int Id { get; set; }

        public int? CustomerId { get; set; }

        public string PayMethod { get; set; }

        public DateTime? SalesDate { get; set; }

        public double? TotalDiscount { get; set; }

        public double? DiscountPercentage { get; set; }

        public double? GrandTotal { get; set; }

        public double? Payment { get; set; }

        public double? Due { get; set; }

        public double? SalesTax { get; set; }

        public string DrivingLicenseNo { get; set; }

        public string InvoiceNo { get; set; }

        public string PaymentStatus { get; set; }

        public int? TerminalId { get; set; }

        public string TerminalName { get; set; }

        public int? PointsEarned { get; set; }

        public int? PointsRedeemed { get; set; }

        public string LoyaltyCard { get; set; }

        public double? InvoiceRedeemAmount { get; set; }

        public PosTerminalDto Terminal { get; set; }

        public IList<TransactionDto> Transactions { get; set; }

        public IList<SalesDetailDto> SalesDetails { get; set; }

        public string Signature { get; set; }

        public double? ReturnAmount { get; set; }
    }
}