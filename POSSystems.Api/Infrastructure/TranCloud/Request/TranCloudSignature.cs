using Newtonsoft.Json;
using POSSystems.Core.Dtos.TranCloud;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudSignature
    {
        public TStream TStream;

        public static string GetTranCloudSignature(TranCloudConfig tranCloudConfig)
        {
            var tranCloudSignature = new TranCloudSignature
            {
                TStream = new TStream
                {
                    Transaction = new Transaction
                    {
                        MerchantID = tranCloudConfig.MerchantID,
                        SecureDevice = "CloudEMV1",
                        TranCode = "GetSignature",
                        TranDeviceID = tranCloudConfig.DeviceID,
                        PinPadIpPort = tranCloudConfig.PinPadIpPort,
                        PinPadMACAddress = tranCloudConfig.PinPadMACAddress,
                        ComPort = tranCloudConfig.ComPort,
                        SequenceNo = tranCloudConfig.SequenceNo
                    }
                }
            };

            var json = JsonConvert.SerializeObject(tranCloudSignature, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return json;
        }
    }
}