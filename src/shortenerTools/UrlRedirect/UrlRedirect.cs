using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;

namespace Cloud5mins.Function
{
    public static class UrlRedirect
    {
        [FunctionName("UrlRedirect")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlRedirect/{shortUrl}")] HttpRequestMessage req,
            string shortUrl,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            string redirectUrl = "https://azure.com";

            if (!String.IsNullOrWhiteSpace(shortUrl))
            {
                redirectUrl = Environment.GetEnvironmentVariable("defaultRedirectUrl");

                StorageTableHelper stgHelper = new StorageTableHelper(Environment.GetEnvironmentVariable("UlsDataStorage"));

                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);

                var newUrl = await stgHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    //log.LogInformation($"Found it: {newUrl.Url}");
                    newUrl.Clicks++;
                    stgHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await stgHelper.SaveShortUrlEntity(newUrl);
                    redirectUrl = newUrl.Url;
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
            }

            return new RedirectResult(redirectUrl);
        }
    }
}
