/*
```c#
Input:


Output:
    {
        "Url": "https://SOME_URL",
        "Clicks": 0,
        "PartitionKey": "d",
        "title": "Quickstart: Create your first function in Azure using Visual Studio"
        "RowKey": "doc",
        "Timestamp": "0001-01-01T00:00:00+00:00",
        "ETag": "W/\"datetime'2020-05-06T14%3A33%3A51.2639969Z'\""
    }
*/

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Cloud5mins.Function
{
    public static class UrlList
    {
        [FunctionName("UrlList")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var result = new ListResponse();
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);

            try
            {
                result.UrlList = await stgHelper.GetAllShortUrlEntities();
                result.UrlList = result.UrlList.Where(p => !(p.IsArchived ?? false)).ToList();
                var host = req.RequestUri.GetLeftPart(UriPartial.Authority);
                foreach (ShortUrlEntity url in result.UrlList)
                {
                    url.ShortUrl = Utility.GetShortUrl(host, url.RowKey);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(ex);
            }

            return new OkObjectResult(result);
        }
    }
}
