using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace UPC.Api.Service;

public static class ServiceBase
{
    [Obsolete("Obsolete")]
    public static string Post(string url, string content, string apiKey, string signature)
    {
        RemoveTrace();
        RemoveServerCertificate();

        // Request
        HttpWebRequest request = WebRequest.CreateHttp(url);
        request.ContentType = "application/json";
        request.Accept = "application/json";
        request.AllowAutoRedirect = true;
        request.Method = WebRequestMethods.Http.Post;
        request.Timeout = 30 * 1000; // 30s

        // Headers
        request.Headers.Add("apiKey", apiKey);
        request.Headers.Add("signature", signature);

        // Post
        using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
        {
            streamWriter.Write(content);
            streamWriter.Flush();
        }

        string response = GetResponse(request);
        return response;
    }

    private static string GetResponse(WebRequest request)
    {
        string content;
        try
        {
            using WebResponse response = request.GetResponse();
            content = GetContent(response);

            return content;
        }
        catch (WebException exception)
        {
            if (exception.Status == WebExceptionStatus.Timeout)
            {
                throw;
            }

            using HttpWebResponse response = (HttpWebResponse) exception.Response!;
            content = GetContent(response);

            return content;
        }
    }

    private static string GetContent(WebResponse? response)
    {
        using Stream? stream = response?.GetResponseStream();
        if (stream == null)
        {
            return string.Empty;
        }

        using StreamReader streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }


    private static bool RemoveCertificateValidate(
        object sender,
        X509Certificate? cert,
        X509Chain? chain,
        SslPolicyErrors error)
    {
        return true;
    }

    private static void RemoveServerCertificate()
    {
        ServicePointManager.ServerCertificateValidationCallback += RemoveCertificateValidate;
    }

    private static void RemoveTrace()
    {
        Activity.Current = null;
    }
}