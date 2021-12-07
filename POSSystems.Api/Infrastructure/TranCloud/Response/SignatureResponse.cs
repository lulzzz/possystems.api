using Newtonsoft.Json;
using System;
using System.Drawing;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public class SignatureResponse
    {
        public string ResponseOrigin;
        public string DSIXReturnCode;
        public string CmdStatus;
        public string TextResponse;
        public string SequenceNo;
        public string UserTrace;
        public string SignMaximumX;
        public string SignMaximumY;
        public string Signature;

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
            signatureResponse.SignMaximumX = rstream["SignMaximumX"];
            signatureResponse.SignMaximumY = rstream["SignMaximumY"];
            signatureResponse.Signature = rstream["Signature"];

            return signatureResponse;
        }

        public string SignatureData
        {
            get
            {
                string base64String = string.Empty;

                // Parse result and save signature to PNG file
                if (CmdStatus == "Success")
                {
                    string points = Signature;
                    string maxX = SignMaximumX;
                    string maxY = SignMaximumY;

                    var pen = new Pen(Color.Black, 3);

                    int prevX = -1, prevY = -1;
                    using (var bmp = new Bitmap(Convert.ToInt32(maxX), Convert.ToInt32(maxY)))
                    {
                        using (var gr = Graphics.FromImage(bmp))
                        {
                            foreach (string stroke in points.Split(new string[] { ":#,#:" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                foreach (string point in stroke.Split(':'))
                                {
                                    string[] coords = point.Split(',');
                                    if (prevX != -1)
                                    {
                                        gr.DrawLine(pen, prevX, prevY, Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]));
                                    }
                                    prevX = Convert.ToInt32(coords[0]);
                                    prevY = Convert.ToInt32(coords[1]);
                                }
                                prevX = -1;
                                prevY = -1;
                            }
                        }

                        using (var stream = new System.IO.MemoryStream())
                        {
                            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] imageBytes = stream.ToArray();

                            // Convert byte[] to Base64 String
                            base64String = Convert.ToBase64String(imageBytes);

                            //bmp.Save("signature.png");
                        }
                    }
                }

                return base64String;
            }
        }
    }
}