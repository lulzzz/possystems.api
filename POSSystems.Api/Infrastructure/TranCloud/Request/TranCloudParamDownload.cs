using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudParamDownload
    {
        public TStream TStream;

        public static string GetTranCloudParamDownload(TranCloudConfig tranCloudConfig)
        {
            var tranCloudTransaction = new TranCloudParamDownload
            {
                TStream = new TStream
                {
                    Transaction = new Transaction
                    {
                        MerchantID = tranCloudConfig.MerchantID,
                        SecureDevice = "CloudEMV1",
                        TranCode = "EMVParamDownload",
                        TranDeviceID = tranCloudConfig.DeviceID,
                        PinPadIpPort = tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = tranCloudConfig.PinPadMACAddress,
                        ComPort = tranCloudConfig.ComPort,
                        SequenceNo = tranCloudConfig.SequenceNo
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