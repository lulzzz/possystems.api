using Humanizer;
using Newtonsoft.Json;
using POSSystems.Core;
using System;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class PaymentInfoResponse
    {
        public string AcqRefData;
        public string AuthCode;
        public string CardholderName;
        public string CardType;
        public string DisplayMessage;
        public string ExpDate;
        public string MaskedAccount;
        public string Token;
        public string Signature;
        public TransactionStatus CmdStatus = TransactionStatus.NotStarted;
        public string DSIXReturnCode;
        public ResponseOrigin ResponseOrigin;
        public double Purchase { get; set; }
        public double Authorize { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.None;
        public string InvoiceNo { get; set; }

        //public string PrintLines { get; private set; }
        public bool SignatureNeeded { get; set; } = false;

        public static PaymentInfoResponse MapTranCloudResponse(string response)
        {
            var tranCloudResponse = JsonConvert.DeserializeObject<dynamic>(response);
            var rstream = tranCloudResponse["RStream"];
            //var tranResponse = rstream["TranResponse"];

            var paymentResponse = new PaymentInfoResponse
            {
                AcqRefData = rstream["AcqRefData"],
                AuthCode = rstream["AuthCode"],
                Purchase = Convert.ToDouble((string)rstream["Purchase"]),
                //paymentResponse.CardholderName = "test test";
                CardType = rstream["CardType"],
                DisplayMessage = rstream["TextResponse"],
                //paymentResponse.ExpDate = rstream["ExpDate"];
                MaskedAccount = rstream["AcctNo"],
                Token = rstream["RecordNo"],
                CmdStatus = ((string)rstream["CmdStatus"]).DehumanizeTo<TransactionStatus>(),
                Authorize = Convert.ToDouble((string)rstream["Authorize"]),
                ResponseOrigin = ((string)rstream["ResponseOrigin"]).DehumanizeTo<ResponseOrigin>(),
                DSIXReturnCode = (string)rstream["DSIXReturnCode"]
            };

            var cvm = rstream["CVM"];
            if (cvm != null)
            {
                if ((string)cvm == "SIGN")
                    paymentResponse.SignatureNeeded = true;
            }
            else if (paymentResponse.CardType == "Debit")
            {
                paymentResponse.SignatureNeeded = true;
            }

            return paymentResponse;
        }
    }
}