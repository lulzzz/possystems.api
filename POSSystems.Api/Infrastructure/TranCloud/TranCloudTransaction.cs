using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Core.Exceptions;
using POSSystems.Web.Controllers;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public abstract class TranCloudTransaction : POSTransaction
    {
        protected ILogger<SalesController> _logger;
        protected TranCloudConfig _tranCloudConfig;

        public TranCloudTransaction(string invoiceNo, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig) : base(invoiceNo)
        {
            _logger = logger;
            _tranCloudConfig = tranCloudConfig;
        }

        public abstract string GetTranCloudTransaction(double amount);

        public override async Task<PaymentInfoResponse> ProcessPayment(double paidTotal, double processingTotal)
        {
            _logger.LogInformation($"{paidTotal} paid before transaction started.");

            var tranjson = GetTranCloudTransaction(processingTotal);
            var response = await TranCloudWebRequest.DoTranCloudRequest(tranjson, _tranCloudConfig, _logger);
            PaymentResponse = PaymentInfoResponse.MapTranCloudResponse(response);

            if (PaymentResponse.CmdStatus == TransactionStatus.Error
                && PaymentResponse.ResponseOrigin == ResponseOrigin.Client
                && PaymentResponse.DSIXReturnCode == "003328")
                throw new POSException("Customer cancelled the transaction.");

            if (PaymentResponse.Authorize == processingTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.Complete;
            else if (PaymentResponse.Authorize == 0 || PaymentResponse.CmdStatus != TransactionStatus.Approved)
                PaymentResponse.PaymentStatus = PaymentStatus.None;
            else if (PaymentResponse.Authorize < processingTotal)
                PaymentResponse.PaymentStatus = PaymentStatus.Partial;

            return PaymentResponse;
        }
    }
}