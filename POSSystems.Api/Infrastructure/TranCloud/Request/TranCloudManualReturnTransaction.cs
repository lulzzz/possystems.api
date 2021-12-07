using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;
using POSSystems.Web.Controllers;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudManualReturnTransaction : TranCloudReturnTransaction
    {
        public TStream TStream;

        public TranCloudManualReturnTransaction(string invoice, ILogger<ReturnController> logger,
            TranCloudConfig tranCloudConfig, string authCode, string acqRefData)
            : base(invoice, logger, tranCloudConfig, authCode, acqRefData)
        {
        }

        public override string GetTranCloudTransaction(double amount)
        {
            var tranCloudTransaction = new TranCloudManualReturnTransaction(_invoiceNo, _logger, _tranCloudConfig, _authCode, _acqRefData)
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
                        RefNo = _invoiceNo,
                        SecureDevice = "CloudEMV1",
                        TranCode = "EMVReturn",
                        TranDeviceID = _tranCloudConfig.DeviceID,
                        PinPadIpPort = _tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = _tranCloudConfig.PinPadMACAddress,
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