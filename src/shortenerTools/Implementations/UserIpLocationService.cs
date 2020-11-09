using shortenerTools.Abstractions;
using shortenerTools.Extensions;
using shortenerTools.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace shortenerTools.Implementations
{
    public class UserIpLocationService : IUserIpLocationService
    {
        private readonly HttpClient _httpClient;

        public UserIpLocationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<UserIpResponse> GetUserIpAsync(CancellationToken cancellationToken)
        {
            return await _httpClient.GetAsync<UserIpResponse>("/json", cancellationToken);
        }
    }
}