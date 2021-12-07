using System;
using System.Collections.Generic;

namespace POSSystems.Core.Dtos.Sales
{
    public class InvoiceDTO
    {
        public double Subtotal { get; set; }
        public double TotalTax { get; set; }
        public double TotalPrice { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public List<InvoiceItemDTO> InvoiceItemList { get; set; }
        public string InvoiceNo { get; set; }
        public string PaymentType { get; set; }
        public double PaidTotal { get; set; }
        public List<InvoicePaymentDTO> InvoicePaymentList { get; set; }
        public string CompanyAddress2 { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public double? Discount { get; set; }
        public string Masked_Account { get; set; }
        public string LoyaltyPointEarned { get; set; }
    }
}