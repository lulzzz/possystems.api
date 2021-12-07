using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;
using POSSystems.Web.Controllers;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudCardTransaction : TranCloudTransaction
    {
        public TStream TStream;

        public TranCloudCardTransaction(string invoice, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig) : base(invoice, logger, tranCloudConfig)
        {
        }

        public override string GetTranCloudTransaction(double amount)
        {
            TranCloudCardTransaction tranCloudTransaction = new TranCloudCardTransaction(_invoiceNo, _logger, _tranCloudConfig)
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
                        PinPadIpPort = _tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = _tranCloudConfig.PinPadMACAddress,
                        Frequency = "OneTime",
                        RecordNo = "RecordNumberRequested",
                        ComPort = _tranCloudConfig.ComPort,
                        SequenceNo = _tranCloudConfig.SequenceNo
                    }
                }
            };

            var json = JsonConvert.SerializeObject(tranCloudTransaction, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }
    }
}