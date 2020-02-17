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
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = System.Net.DecompressionMethods.All;
            HttpClient client = new HttpClient(handler);
            ProductHeaderValue header = new ProductHeaderValue("AudicaDownloader", Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ProductInfoHeaderValue userAgent = new ProductInfoHeaderValue(header);
            client.DefaultRequestHeaders.UserAgent.Add(userAgent);
            return client;
        }
    }
}
