/*
```c#
Input:

    {
        // [Required] The url you wish to have a short version for
        "url": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio",
        
        // [Optional] Title of the page, or text description of your choice.
        "title": "Quickstart: Create your first function in Azure using Visual Studio"

        // [Optional] the end of the URL. If nothing one will be generated for you.
        "vanity": "azFunc"
    }

Output:
    {
        "ShortUrl": "http://c5m.ca/azFunc",
        "LongUrl": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio"
    }
*/

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Cloud5mins.domain;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Cloud5mins.Function
{

    public static class UrlShortener
    {

        [FunctionName("UrlShortener")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
        ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");
            var x = req.RequestUri.GetLeftPart(UriPartial.Authority);
            // Validation of the inputs
            if (req == null)
            {
                return new NotFoundResult();
            }

            ShortRequest input = await req.Content.ReadAsAsync<ShortRequest>();
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

            var result = new ShortResponse();

            StorageTableHelper stgHelper = new StorageTableHelper(Environment.GetEnvironmentVariable("UlsDataStorage"));

            try
            {
                string longUrl = input.Url.Trim();
                string vanity = string.IsNullOrWhiteSpace(input.Vanity) ? "" : input.Vanity.Trim();
                string title = string.IsNullOrWhiteSpace(input.Title) ? "" : input.Title.Trim();

                ShortUrlEntity newRow;

                if (!string.IsNullOrEmpty(vanity))
                {
                    newRow = new ShortUrlEntity(longUrl, vanity, title);
                    if (await stgHelper.IfShortUrlEntityExist(newRow))
                    {
                        return new ConflictObjectResult("This Short URL already exist.");
                    }
                }
                else
                {
                    newRow = new ShortUrlEntity(longUrl, await Utility.GetValidEndUrl(vanity, stgHelper), title);
                }

                await stgHelper.SaveShortUrlEntity(newRow);

                string host = null;
                if (req.Headers.TryGetValues("X-Forwarded-Host", out IEnumerable<string> hosts))
                {
                    var builder = new UriBuilder(req.RequestUri);
                    builder.Host = hosts.ToArray().First();
                    host = builder.Uri.GetLeftPart(UriPartial.Authority);
                }
                if (string.IsNullOrEmpty(host)) { host = req.RequestUri.GetLeftPart(UriPartial.Authority); }
                result = new ShortResponse(host, newRow.Url, newRow.RowKey, newRow.Title);

                log.LogInformation("Short Url created.");
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
