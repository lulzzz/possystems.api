using POSSystems.Core;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public abstract class ReturnTransaction
    {
        protected string _invoiceNo;

        public ReturnTransaction(string invoiceNo)
        {
            _invoiceNo = invoiceNo;
            PaymentResponse = new PaymentInfoResponse { InvoiceNo = invoiceNo };
        }

        public PaymentInfoResponse PaymentResponse { get; set; }

        public virtual async Task<double> ProcessPayment(double returnAmount, double invoiceTotal, double? amountReturnedYet)
        {
            var returnedTotal = amountReturnedYet.HasValue ? amountReturnedYet.Value + returnAmount : returnAmount;
            PaymentResponse.Purchase = returnAmount;
            PaymentResponse.Authorize = returnAmount;

            if (returnedTotal == invoiceTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.CompletelyReturned;
            else if (returnAmount == 0)
                PaymentResponse.PaymentStatus = PaymentStatus.None;
            else if (returnedTotal < invoiceTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.PartiallyReturned;

            return returnedTotal;
        }
    }

    public class ReturnBasicTransaction : ReturnTransaction
    {
        public ReturnBasicTransaction(string invoiceNo) : base(invoiceNo)
        {
        }
    }
}