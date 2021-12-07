using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;
using POSSystems.Web.Controllers;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudFSATransaction : TranCloudTransaction
    {
        private readonly double _rxAmount;

        public TStream TStream;

        public TranCloudFSATransaction(string invoice, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig, double rxAmount) : base(invoice, logger, tranCloudConfig)
        {
            _rxAmount = rxAmount;
        }

        public override string GetTranCloudTransaction(double amount)
        {
            var tranCloudTransaction = new TranCloudFSATransaction(_invoiceNo, _logger, _tranCloudConfig, _rxAmount)
            {
                TStream = new TStream
                {
                    Transaction = new Transaction
                    {
                        Amount = new Amount
                        {
                            Purchase = string.Format("{0:0.00}", amount)
                        },
                        InvoiceNo = _invoiceNo,
                        MerchantID = _tranCloudConfig.MerchantID,
                        PartialAuth = "Allow",
                        RefNo = _invoiceNo,
                        SecureDevice = "CloudEMV1",
                        TranCode = "EMVSale",
                        TranDeviceID = _tranCloudConfig.DeviceID,
                        TranType = "FSA",
                        PinPadIpPort = _tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = _tranCloudConfig.PinPadMACAddress,
                        Frequency = "OneTime",
                        RecordNo = "RecordNumberRequested",
                        ComPort = _tranCloudConfig.ComPort,
                        SequenceNo = _tranCloudConfig.SequenceNo,
                        FSAPrescription = string.Format("{0:0.00}", _rxAmount)
                    }
                }
            };

            if (_tranCloudConfig.TestMode == "On")
            {
                tranCloudTransaction.TStream.Transaction.Account = new Account
                {
                    AcctNo = "Prompt"
                };
            }

            var json = JsonConvert.SerializeObject(tranCloudTransaction, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }
    }
}