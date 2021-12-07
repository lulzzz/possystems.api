using Newtonsoft.Json;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class ResetResponse
    {
        public string ResponseOrigin;
        public string DSIXReturnCode;
        public string CmdStatus;
        public string TextResponse;
        public string SequenceNo;
        public string UserTrace;

        public static ResetResponse MapTranCloudResponse(string response)
        {
            var tranCloudResponse = JsonConvert.DeserializeObject<dynamic>(response);
            var rstream = tranCloudResponse["RStream"];

            var resetResponse = new ResetResponse();

            resetResponse.ResponseOrigin = rstream["ResponseOrigin"];
            resetResponse.DSIXReturnCode = rstream["DSIXReturnCode"];
            resetResponse.CmdStatus = rstream["CmdStatus"];
            resetResponse.TextResponse = rstream["TextResponse"];
            resetResponse.SequenceNo = rstream["SequenceNo"];
            resetResponse.UserTrace = rstream["UserTrace"];

            return resetResponse;
        }
    }
}