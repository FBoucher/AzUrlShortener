/*
```c#
Input:
    {
         // [Required]
        "PartitionKey": "d",

         // [Required]
        "RowKey": "doc",

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
    public class UrlArchive : FunctionBase
    {
        private readonly IStorageTableHelper _storageTableHelper;

        public UrlArchive(IStorageTableHelper storageTableHelper)
        {
            _storageTableHelper = storageTableHelper;
        }

        [FunctionName("UrlArchive")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
        ILogger log,
        ExecutionContext context,
        ClaimsPrincipal principal)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            var (requestValid, invalidResult, shortUrlEntity) = await ValidateRequestAsync<ShortUrlEntity>(context, req, principal, log);

            if (!requestValid)
            {
                return invalidResult;
            }

            try
            {
                shortUrlEntity = await _storageTableHelper.ArchiveShortUrlEntity(shortUrlEntity);
                return new OkObjectResult(shortUrlEntity);
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
