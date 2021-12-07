using POSSystems.Core;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public abstract class POSTransaction
    {
        protected string _invoiceNo;

        public POSTransaction(string invoiceNo)
        {
            _invoiceNo = invoiceNo;
            PaymentResponse = new PaymentInfoResponse { InvoiceNo = invoiceNo };
        }

        public PaymentInfoResponse PaymentResponse { get; set; }

        public virtual async Task<PaymentInfoResponse> ProcessPayment(double paidTotal, double toBeProcessedTotal)
        {
            PaymentResponse.Purchase = paidTotal;
            PaymentResponse.Authorize = paidTotal;

            if (paidTotal >= toBeProcessedTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.Complete;
            else if (paidTotal == 0)
                PaymentResponse.PaymentStatus = PaymentStatus.None;
            else if (paidTotal < toBeProcessedTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.Partial;

            return PaymentResponse;
        }
    }

    public class POSBasicTransaction : POSTransaction
    {
        public POSBasicTransaction(string invoiceNo) : base(invoiceNo)
        {
        }
    }
}