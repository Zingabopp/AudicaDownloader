using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Reflection;
using System.Net.Http.Headers;

namespace AudicaDownloader
{
    public static class AudicaHttpClient
    {
        public static HttpClient GetClient()
        {
            HttpClient client = new HttpClient();
            ProductHeaderValue header = new ProductHeaderValue("AudicaDownloader", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ProductInfoHeaderValue userAgent = new ProductInfoHeaderValue(header);
            client.DefaultRequestHeaders.UserAgent.Add(userAgent);
            return client;
        }
    }
}
