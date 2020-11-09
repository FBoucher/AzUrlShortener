using Cloud5mins.domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace shortenerTools.Abstractions
{
    public abstract class FunctionBase
    {
        public virtual async Task<(bool isValidRequest, IActionResult invalidResult, T requestType)> ValidateRequestAsync<T>(
            ExecutionContext context, HttpRequestMessage req, ClaimsPrincipal principal, ILogger log) where T : class, new()
        {
            var invalidRequest = Utility.CatchUnauthorized(principal, log);
            if (invalidRequest != null)
            {
                return (false, invalidRequest, null as T);
            }

            LogAuthenticatedUser(principal, context, log);

            if (req == null)
            {
                return (false, new BadRequestObjectResult(new { StatusCode = HttpStatusCode.NotFound }), null);
            }

            var result = await req.Content.ReadAsAsync<T>();
            if (result == null)
            {
                return (false, new NotFoundResult(), null);
            }

            return (true, null, result);
        }

        protected static void LogAuthenticatedUser(ClaimsPrincipal principal, ExecutionContext context, ILogger log)
        {
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
            log.LogInformation("{functionName} successfully Authenticated user {user}.", context.FunctionName, userId);
        }
    }
}