using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Demo.Service
{
    public static class ServiceBase
    {
        public static string Post(string url, string content, string merchantKey)
        {
            RemoveServerCertificate();

            // Request
            System.Net.HttpWebRequest request = WebRequest.CreateHttp(url);
            request.ContentType = "application/json";
            request.AllowAutoRedirect = true;
            request.Accept = "application/json";
            request.Method = WebRequestMethods.Http.Post;
            request.Timeout = 30 * 1000; // 30s

            // Headers
            request.Headers.Add("merchantKey", merchantKey);

            // Post
            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(content);
                streamWriter.Flush();
            }

            string response = GetResponse(request);
            return response;
        }

        private static string GetResponse(HttpWebRequest request)
        {
            string content;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    content = GetContent(response);
                }

                return content;
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    throw;
                }

                using (HttpWebResponse response = (HttpWebResponse)exception.Response)
                {
                    content = GetContent(response);
                }

                return content;
            }
        }

        private static string GetContent(WebResponse response)
        {
            if (response == null)
            {
                return string.Empty;
            }

            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null)
                {
                    return string.Empty;
                }

                using (StreamReader streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }


        public static bool RemoveCertificateValidate(
            object sender,
            X509Certificate cert,
            X509Chain chain,
            SslPolicyErrors error)
        {
            return true;
        }

        public static void RemoveServerCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback += RemoveCertificateValidate;
        }
    }
}
