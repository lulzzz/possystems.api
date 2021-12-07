using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;
using POSSystems.Web.Controllers;
using System;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudManualFSAReturnTransaction : TranCloudReturnTransaction
    {
        public TStream TStream;

        public TranCloudManualFSAReturnTransaction(string invoice, ILogger<ReturnController> logger, TranCloudConfig tranCloudConfig, string authCode, string acqRefData)
            : base(invoice, logger, tranCloudConfig, authCode, acqRefData)
        {
        }

        public override string GetTranCloudTransaction(double amount)
        {
            var tranCloudTransaction = new TranCloudManualFSAReturnTransaction(_invoiceNo, _logger, _tranCloudConfig, _authCode, _acqRefData)
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
                        AuthCode = _authCode,
                        AcqRefData = _acqRefData,
                        MerchantID = _tranCloudConfig.MerchantID,
                        RefNo = _invoiceNo,
                        SecureDevice = "CloudEMV1",
                        TranCode = "EMVVoidSale",
                        TranDeviceID = _tranCloudConfig.DeviceID,
                        PinPadIpPort = _tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = _tranCloudConfig.PinPadMACAddress,
                        ComPort = _tranCloudConfig.ComPort,
                        SequenceNo = _tranCloudConfig.SequenceNo,
                        TranType = "FSA"
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