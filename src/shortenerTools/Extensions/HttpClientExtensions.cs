using Newtonsoft.Json;
using shortenerTools.Exceptions;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace shortenerTools.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<TResponse> GetAsync<TResponse>(this HttpClient httpClient, string queryUrl, CancellationToken cancellationToken = default)
        {
            return await CreateRequestAndSendAsync<TResponse>(httpClient, HttpMethod.Get, queryUrl,
                cancellationToken: cancellationToken);
        }

        private static async Task<TResponse> CreateRequestAndSendAsync<TResponse>(this HttpMessageInvoker httpClient,
            HttpMethod httpMethod, string queryUrl, object requestBody = null,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(httpMethod, queryUrl);

            if (requestBody != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                    "application/json");
            }

            using var response = await httpClient.SendAsync(request, cancellationToken);

            var stream = await response.Content.ReadAsStreamAsync();

            if (response.IsSuccessStatusCode)
                return stream.DeserializeJsonFromStream<TResponse>();

            var content = await stream.StreamToStringAsync();

            throw new HttpClientException(response.RequestMessage.RequestUri.ToString(),
                response.RequestMessage.Method.Method,
                (int)response.StatusCode,
                content,
                response.RequestMessage.Content != null
                    ? await response.RequestMessage.Content.ReadAsStringAsync()
                    : string.Empty);
        }
    }
}