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

using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{

    public class UrlShortener : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlShortener(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlShortener")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            // Validation of the inputs
            var (requestValid, invalidResult, shortRequest) = await ValidateRequestAsync<ShortRequest>(context, req, principal, log);

            if (!requestValid)
            {
                return invalidResult;
            }
            
            // If the Url parameter only contains whitespaces or is empty return with BadRequest.
            if (string.IsNullOrWhiteSpace(shortRequest.Url))
            {
                return new BadRequestObjectResult("The url parameter can not be empty.");
            }

            // Validates if input.url is a valid absolute url, aka is a complete reference to the resource, ex: http(s)://google.com
            if (!Uri.IsWellFormedUriString(shortRequest.Url, UriKind.Absolute))
            {
                return new BadRequestObjectResult($"{shortRequest.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'.");
            }

            try
            {
                var longUrl = shortRequest.Url.Trim();
                var vanity = string.IsNullOrWhiteSpace(shortRequest.Vanity) ? "" : shortRequest.Vanity.Trim();
                var title = string.IsNullOrWhiteSpace(shortRequest.Title) ? "" : shortRequest.Title.Trim();

                ShortUrlEntity newRow;

                if (!string.IsNullOrEmpty(vanity))
                {
                    newRow = new ShortUrlEntity(longUrl, vanity, title);
                    if (await _storageTableHelper.IfShortUrlEntityExist(newRow))
                    {
                        return new ConflictObjectResult("This Short URL already exist.");
                    }
                }
                else
                {
                    newRow = new ShortUrlEntity(longUrl, await Utility.GetValidEndUrl(vanity, _storageTableHelper), title);
                }

                await _storageTableHelper.SaveShortUrlEntity(newRow);

                var host = req.RequestUri.GetLeftPart(UriPartial.Authority);
                log.LogInformation($"-> host = {host}");

                var result = new ShortResponse(host, newRow.Url, newRow.RowKey, newRow.Title);

                log.LogInformation("Short Url created.");

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "{functionName} failed due to an unexpected error: {errorMessage}.",
                    context.FunctionName, ex.GetBaseException().Message);

                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }
        }
    }
}
