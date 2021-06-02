/*
```c#
Input:
    {
         // [Required]
        "PartitionKey": "d",

         // [Required]
        "RowKey": "doc",

        // [Optional] New Title for this URL, or text description of your choice.
        "title": "Quickstart: Create your first function in Azure using Visual Studio"

        // [Optional] New long Url where the the user will be redirect
        "Url": "https://SOME_URL"
    }


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
using Microsoft.AspNetCore.Mvc;

namespace Cloud5mins.Function
{
    public static class UrlUpdate
    {
        [FunctionName("UrlUpdate")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            if (req == null)
            {
                return new NotFoundResult();
            }

            ShortUrlEntity input = await req.Content.ReadAsAsync<ShortUrlEntity>();
            if (input == null)
            {
                return new NotFoundResult();
            }


            // If the Url parameter only contains whitespaces or is empty return with BadRequest.
            if (string.IsNullOrWhiteSpace(input.Url))
            {
                return new BadRequestObjectResult("The url parameter can not be empty.");
            }

            // Validates if input.url is a valid aboslute url, aka is a complete refrence to the resource, ex: http(s)://google.com
            if (!Uri.IsWellFormedUriString(input.Url, UriKind.Absolute))
            {
                return new BadRequestObjectResult($"{input.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'.");
            }

            ShortUrlEntity result;

            StorageTableHelper stgHelper = new StorageTableHelper(Environment.GetEnvironmentVariable("UlsDataStorage"));

            try
            {
                result = await stgHelper.UpdateShortUrlEntity(input);
                var host = req.RequestUri.GetLeftPart(UriPartial.Authority);
                result.ShortUrl = Utility.GetShortUrl(host, result.RowKey);

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
