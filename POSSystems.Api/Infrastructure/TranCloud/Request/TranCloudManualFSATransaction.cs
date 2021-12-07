using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;
using POSSystems.Web.Controllers;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudManualFSATransaction : TranCloudTransaction
    {
        public TStream TStream;

        public TranCloudManualFSATransaction(string invoice, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig) : base(invoice, logger, tranCloudConfig)
        {
        }

        public override string GetTranCloudTransaction(double amount)
        {
            var tranCloudTransaction = new TranCloudManualFSATransaction(_invoiceNo, _logger, _tranCloudConfig)
            {
                TStream = new TStream
                {
                    Transaction = new Transaction
                    {
                        Account = new Account
                        {
                            AcctNo = "Prompt"
                        },
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