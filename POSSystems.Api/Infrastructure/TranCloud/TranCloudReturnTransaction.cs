using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Web.Controllers;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public abstract class TranCloudReturnTransaction : ReturnTransaction
    {
        protected ILogger<ReturnController> _logger;
        protected TranCloudConfig _tranCloudConfig;
        protected string _authCode;
        protected string _acqRefData;

        public TranCloudReturnTransaction(string invoiceNo, ILogger<ReturnController> logger, TranCloudConfig tranCloudConfig, string authCode, string acqRefData) : base(invoiceNo)
        {
            _logger = logger;
            _tranCloudConfig = tranCloudConfig;
            _authCode = authCode;
            _acqRefData = acqRefData;
        }

        public abstract string GetTranCloudTransaction(double amount);

        public override async Task<double> ProcessPayment(double returnAmount, double invoiceTotal, double? amountReturnedYet)
        {
            _logger.LogInformation($"{returnAmount} paid before transaction started.");

            var tranjson = GetTranCloudTransaction(returnAmount);
            var response = await TranCloudWebRequest.DoTranCloudRequest(tranjson, _tranCloudConfig, _logger);
            PaymentResponse = PaymentInfoResponse.MapTranCloudResponse(response);

            var returnedTotal = amountReturnedYet.HasValue ? amountReturnedYet.Value + PaymentResponse.Authorize : PaymentResponse.Authorize;
            //PaymentResponse.AmountAttended = returnedTotal;

            if (PaymentResponse.Authorize == 0 || PaymentResponse.CmdStatus != TransactionStatus.Approved)
                PaymentResponse.PaymentStatus = PaymentStatus.None;
            else if (returnedTotal < invoiceTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.PartiallyReturned;
            else if (returnedTotal == invoiceTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.CompletelyReturned;

            return returnedTotal;
        }
    }
}