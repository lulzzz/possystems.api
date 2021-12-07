using Microsoft.Extensions.Logging;
using POSSystems.Core;
using POSSystems.Web.Controllers;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class TranCloudResetHandler
    {
        private readonly bool _trancloudEnabled;
        private readonly TranCloudConfig _tranCloudConfig;
        private readonly ILogger<SalesController> _logger;

        public TranCloudResetHandler(bool trancloudEnabled, ILogger<SalesController> logger, TranCloudConfig tranCloudConfig)
        {
            _tranCloudConfig = tranCloudConfig;
            _trancloudEnabled = trancloudEnabled;
            _logger = logger;
        }

        public async Task ResetPinpad()
        {
            try
            {
                if (_trancloudEnabled)
                {
                    var resetjson = TranCloudReset.GetTranCloudReset(_tranCloudConfig);
                    var sresponse = await TranCloudWebRequest.DoTranCloudRequest(resetjson, _tranCloudConfig, _logger);
                    var resetResponse = SignatureResponse.MapTranCloudResponse(sresponse);
                }
            }
            catch
            {
                _logger.LogError("Pinpad reset not working.");
            }
        }

        public async Task PerformAfterTransactionReset(PaymentType paymentType)
        {
            if (paymentType == PaymentType.Card
                || paymentType == PaymentType.FSA
                || paymentType == PaymentType.Manual
                || paymentType == PaymentType.Manualfsa)
                await ResetPinpad();
        }
    }
}