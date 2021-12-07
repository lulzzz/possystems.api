using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Web.Controllers;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudSignHandler
    {
        private bool _trancloudEnabled = false;
        private TranCloudConfig _tranCloudConfig;
        private ILogger<SalesController> _logger;

        public TranCloudSignHandler(bool trancloudEnabled, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig)
        {
            _tranCloudConfig = tranCloudConfig;
            _trancloudEnabled = trancloudEnabled;
            _logger = logger;
        }

        public async Task<string> GetSignature()
        {
            var signaturejson = TranCloudSignature.GetTranCloudSignature(_tranCloudConfig);
            var sresponse = await TranCloudWebRequest.DoTranCloudRequest(signaturejson, _tranCloudConfig, _logger);
            var signatureResponse = SignatureResponse.MapTranCloudResponse(sresponse);

            if (signatureResponse.CmdStatus == "Success") return "data:image/png;base64," + signatureResponse.SignatureData;

            return null;
        }
    }
}