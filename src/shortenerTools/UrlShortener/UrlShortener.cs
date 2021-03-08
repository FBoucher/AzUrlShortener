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

        // [Optional] DateTime when to automaticly archive the short url.
        "expire": "2021-01-02T17:01:13Z"
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
using System.Net;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using shortenerTools.Domain;

namespace Cloud5mins.Function
{

    public static class UrlShortener
    {

        [FunctionName("UrlShortener")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");
            string userId = string.Empty;
            ShortRequest input;
            var result = new ShortResponse();

            try
            {
                var invalidRequest = Utility.CatchUnauthorize(principal, log);

                if (invalidRequest != null)
                {
                    return invalidRequest;
                }
                else
                {
                    userId = principal.FindFirst(ClaimTypes.GivenName).Value;
                    log.LogInformation("Authenticated user {user}.", userId);
                }

                // Validation of the inputs
                if (req == null)
                {
                    return new BadRequestObjectResult(new { StatusCode = HttpStatusCode.NotFound });
                }

                using (var reader = new StreamReader(req.Body))
                {
                    var strBody = reader.ReadToEnd();
                    input = JsonSerializer.Deserialize<ShortRequest>(strBody, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                    if (input == null)
                    {
                        return new BadRequestObjectResult(new { StatusCode = HttpStatusCode.NotFound });
                    }
                }

                // If the Url parameter only contains whitespaces or is empty return with BadRequest.
                if (string.IsNullOrWhiteSpace(input.Url))
                {
                    return new BadRequestObjectResult(new { StatusCode = HttpStatusCode.BadRequest, Message = "The url parameter can not be empty." });
                }

                // Validates if input.url is a valid aboslute url, aka is a complete refrence to the resource, ex: http(s)://google.com
                if (!Uri.IsWellFormedUriString(input.Url, UriKind.Absolute))
                {
                    return new BadRequestObjectResult(new
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = $"{input.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'."
                    });
                }

                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);


                string longUrl = input.Url.Trim();
                string vanity = string.IsNullOrWhiteSpace(input.Vanity) ? "" : input.Vanity.Trim();
                string title = string.IsNullOrWhiteSpace(input.Title) ? "" : input.Title.Trim();

                ShortUrlEntity newRow;

                if (!string.IsNullOrEmpty(vanity))
                {
                    newRow = new ShortUrlEntity(longUrl, vanity, title);
                    if (await stgHelper.IfShortUrlEntityExist(newRow))
                    {
                        return new ConflictObjectResult(new{ Message = "This Short URL already exist."});
                    }
                }
                else
                {
                    newRow = new ShortUrlEntity(longUrl, await Utility.GetValidEndUrl(vanity, stgHelper), title);
                }

                await stgHelper.SaveShortUrlEntity(newRow);
                var host = string.IsNullOrEmpty(config["customDomain"]) ? req.Host.Host : config["customDomain"].ToString();

                DateTime expire = input.Expire.ToUniversalTime();

                if (expire != DateTime.MinValue)
                {
                    if (expire <= DateTime.UtcNow)
                    {
                        return new BadRequestObjectResult(new
                        {
                            message = $"The expire parameter has already passed. We are not a time traveler ;)",
                            StatusCode = HttpStatusCode.BadRequest
                        });
                    }
                    await using (ServiceBusClient client = new ServiceBusClient(config["ServiceBusConnection"]))
                    {
                        ServiceBusSender sender = client.CreateSender(config["ServiceBusQueName"]);
                        ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(newRow))
                        {
                            ScheduledEnqueueTime = expire,
                        };
                        await sender.SendMessageAsync(message);

                    }

                    log.LogInformation("Short Url with automatic archiving created");
                    return new OkObjectResult(new ShortResponseWithExpirationDate(host, newRow.Url, newRow.RowKey, newRow.Title, expire));
                }

                result = new ShortResponse(host, newRow.Url, newRow.RowKey, newRow.Title);

                log.LogInformation("Short Url created.");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode =  HttpStatusCode.BadRequest
                });
            }

            return new OkObjectResult(result);
        }
    }
}
