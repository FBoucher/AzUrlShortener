using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using shortenerTools.Abstractions;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cloud5mins.domain
{
    public static class Utility
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly int Base = Alphabet.Length;

        public static async Task<string> GetValidEndUrl(string vanity, IStorageTableHelper stgHelper)
        {
            if (!string.IsNullOrEmpty(vanity)) return string.Join(string.Empty, vanity);

            var newKey = await stgHelper.GetNextTableId();
            string GetCode() => Encode(newKey);
            return string.Join(string.Empty, GetCode());
        }

        public static string Encode(int i)
        {
            if (i == 0)
                return Alphabet[0].ToString();
            var s = string.Empty;
            while (i > 0)
            {
                s += Alphabet[i % Base];
                i = i / Base;
            }

            return string.Join(string.Empty, s.Reverse());
        }

        public static string GetShortUrl(string host, string vanity)
        {
            return host + "/" + vanity;
        }

        public static IActionResult CatchUnauthorized(ClaimsPrincipal principal, ILogger log)
        {
            if (principal == null)
            {
                log.LogWarning("No principal.");
                return new UnauthorizedResult();
            }

            if (principal.Identity == null)
            {
                log.LogWarning("No identity.");
                return new UnauthorizedResult();
            }

            if (!principal.Identity.IsAuthenticated)
            {
                log.LogWarning("Request was not authenticated.");
                return new UnauthorizedResult();
            }

            if (principal.FindFirst(ClaimTypes.GivenName) is { }) return null;

            log.LogError("Claim not Found");

            return new BadRequestObjectResult(new
            {
                message = "Claim not Found",
                StatusCode = System.Net.HttpStatusCode.BadRequest
            });
        }

        public static IActionResult CatchUnauthorize(ClaimsPrincipal principal, ILogger log)
        {
            if (principal == null)
            {
                log.LogWarning("No principal.");
                return new UnauthorizedResult();
            }

            if (principal.Identity == null)
            {
                log.LogWarning("No identity.");
                return new UnauthorizedResult();
            }

            if (!principal.Identity.IsAuthenticated)
            {
                log.LogWarning("Request was not authenticated.");
                return new UnauthorizedResult();
            }

            if (principal.FindFirst(ClaimTypes.GivenName) is null)
            {
                log.LogError("Claim not Found");
                return new BadRequestObjectResult(new
                {
                    message = "Claim not Found",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                });
            }
            return null;
        }
    }
}