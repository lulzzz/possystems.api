using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudReset
    {
        public TStream TStream;

        public static string GetTranCloudReset(TranCloudConfig tranCloudConfig)
        {
            var tranCloudReset = new TranCloudReset
            {
                TStream = new TStream
                {
                    Transaction = new Transaction
                    {
                        MerchantID = tranCloudConfig?.MerchantID,
                        SecureDevice = "CloudEMV2",
                        TranCode = "EMVPadReset",
                        TranDeviceID = tranCloudConfig.DeviceID,
                        PinPadIpPort = tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = tranCloudConfig.PinPadMACAddress,
                        ComPort = tranCloudConfig.ComPort,
                        SequenceNo = tranCloudConfig.SequenceNo
                    }
                }
            };

            var json = JsonConvert.SerializeObject(tranCloudReset, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }
    }
}