using POSSystems.Core;

namespace POSSystems.Web.Infrastructure
{
    public class SalesResponse
    {
        public string InvoiceNo { get; set; }
        public double InvoiceTotal { get; set; }
        public double AmountAttended { get; set; }
        public double Authorize { get; set; }
        public double Purchase { get; set; }
        public double Due { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.None;
    }
}