using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;

namespace Cloud5mins.Function
{
    public static class UrlQRRedirect
    {
        [FunctionName("UrlQRRedirect")]
        public static HttpResponseMessage Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlQRRedirect/{shortUrl}")] HttpRequestMessage req,
            string shortUrl, 
            ExecutionContext context,
            ILogger log)
        {

           log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

           var redirectUrl = "http://api.qrserver.com/v1/create-qr-code/?color=000000&amp;bgcolor=FFFFFF&amp;data="+WebUtility.UrlEncode(req.RequestUri.AbsoluteUri)+"&amp;qzone=0&amp;margin=0&amp;size=250x250&amp;ecc=L";

           var res = req.CreateResponse(HttpStatusCode.Redirect);
           res.Headers.Add("Location", redirectUrl);
           return res;
           }
    }      
}
