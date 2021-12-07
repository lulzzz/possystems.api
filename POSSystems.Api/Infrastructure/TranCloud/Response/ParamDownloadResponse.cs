using Newtonsoft.Json;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class ParamDownloadResponse
    {
        public string ResponseOrigin;
        public string DSIXReturnCode;
        public string CmdStatus;
        public string TextResponse;
        public string SequenceNo;
        public string UserTrace;

        public static SignatureResponse MapTranCloudResponse(string response)
        {
            var tranCloudResponse = JsonConvert.DeserializeObject<dynamic>(response);
            var rstream = tranCloudResponse["RStream"];

            var signatureResponse = new SignatureResponse();

            signatureResponse.ResponseOrigin = rstream["ResponseOrigin"];
            signatureResponse.DSIXReturnCode = rstream["DSIXReturnCode"];
            signatureResponse.CmdStatus = rstream["CmdStatus"];
            signatureResponse.TextResponse = rstream["TextResponse"];
            signatureResponse.SequenceNo = rstream["SequenceNo"];
            signatureResponse.UserTrace = rstream["UserTrace"];

            return signatureResponse;
        }
    }
}