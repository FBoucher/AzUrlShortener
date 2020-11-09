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

using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cloud5mins.Function
{
    public class UrlList : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlList(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlList")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var (requestValid, invalidResult, result) = await ValidateRequestAsync<ListResponse>(context, req, principal, log);

            if (!requestValid)
            {
                return invalidResult;
            }

            try
            {
                result.UrlList = await _storageTableHelper.GetAllShortUrlEntities();
                result.UrlList = result.UrlList.Where(p => !(p.IsArchived ?? false)).ToList();
                var host = req.RequestUri.GetLeftPart(UriPartial.Authority);
                foreach (var shortUrl in result.UrlList)
                {
                    shortUrl.ShortUrl = Utility.GetShortUrl(host, shortUrl.RowKey);
                }

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

        public override async Task<(bool isValidRequest, IActionResult invalidResult, T requestType)> ValidateRequestAsync<T>(ExecutionContext context, HttpRequestMessage req, ClaimsPrincipal principal, ILogger log)
        {
            var invalidRequest = Utility.CatchUnauthorized(principal, log);
            if (invalidRequest != null)
            {
                return (false, invalidRequest, null as T);
            }

            LogAuthenticatedUser(principal, context, log);

            return (true, null, new T());
        }
    }
}
