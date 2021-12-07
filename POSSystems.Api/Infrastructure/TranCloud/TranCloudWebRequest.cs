using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace POSSystems.Web.Infrastructure.TranCloud
{
    public static class TranCloudWebRequest
    {
        public async static Task<string> DoTranCloudRequest(string json, TranCloudConfig tranCloudConfig, ILogger<Controller> logger)
        {
            if (tranCloudConfig == null) throw new ArgumentNullException(nameof(tranCloudConfig));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(tranCloudConfig?.Url));
            request.Method = "POST";
            request.ContentType = "application/json";

            var encoded = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(
                tranCloudConfig.Username + ":" + tranCloudConfig.Password));

            request.Headers["Authorization"] = "Basic " + encoded;
            request.Headers["User-Trace"] = "POS Systems";
            logger.LogInformation(json);
            using (var webStream = await request.GetRequestStreamAsync())
            using (var requestWriter = new StreamWriter(webStream, System.Text.Encoding.ASCII))
            {
                await requestWriter.WriteAsync(json);
            }

            var response = string.Empty;

            try
            {
                request.Timeout = 1000000;
                var webResponse = await request.GetResponseAsync();

                using var webStream = webResponse.GetResponseStream();
                if (webStream != null)
                {
                    using StreamReader responseReader = new StreamReader(webStream);
                    response = await responseReader.ReadToEndAsync();
                    logger.LogInformation(response);
                }
            }
            catch (Exception e)
            {
                response = e.ToString();
                logger.LogError(response);
            }

            return response;
        }
    }
}