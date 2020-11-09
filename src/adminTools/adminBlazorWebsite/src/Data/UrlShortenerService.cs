using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace adminBlazorWebsite.Data
{
    public class UrlShortenerService
    {
        public IConfigurationRoot Config { get; set; }
        public UrlShortenerService()
        {
            Config = GetConfiguration();
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            return config;
        }

        private static string GetFunctionUrl(string functionName)
        {
            var funcUrl = new StringBuilder(GetConfiguration()["azFunctionUrl"]);
            funcUrl.Append("/api/");
            funcUrl.Append(functionName);

            var code = GetConfiguration()["code"];
            if (string.IsNullOrWhiteSpace(code)) return funcUrl.ToString();

            funcUrl.Append("?code=");
            funcUrl.Append(code);

            return funcUrl.ToString();
        }

        public async Task<ShortUrlList> GetUrlList()
        {
            var url = GetFunctionUrl("UrlList");

            CancellationToken cancellationToken;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var resultList = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ShortUrlList>(resultList);
        }



        public async Task<ShortUrlList> CreateShortUrl(ShortUrlRequest shortUrlRequest)
        {
            var url = GetFunctionUrl("UrlShortener");

            CancellationToken cancellationToken;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            using var httpContent = CreateHttpContent(shortUrlRequest);
            request.Content = httpContent;

            using var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var resultList = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ShortUrlList>(resultList);
        }


        public async Task<ShortUrlEntity> UpdateShortUrl(ShortUrlEntity editedUrl)
        {
            var url = GetFunctionUrl("UrlUpdate");

            CancellationToken cancellationToken;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            using var httpContent = CreateHttpContent(editedUrl);
            request.Content = httpContent;

            using var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var resultList = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ShortUrlEntity>(resultList);
        }

        public async Task<ShortUrlEntity> ArchiveShortUrl(ShortUrlEntity archivedUrl)
        {
            var url = GetFunctionUrl("UrlArchive");

            CancellationToken cancellationToken;

            using var client = new HttpClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            using var httpContent = CreateHttpContent(archivedUrl);
            request.Content = httpContent;

            using var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            var resultList = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ShortUrlEntity>(resultList);
        }


        private static StringContent CreateHttpContent(object content)
        {
            if (content == null) return null;

            var jsonString = JsonConvert.SerializeObject(content);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

            return httpContent;
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            using var sw = new StreamWriter(stream, new UTF8Encoding(false), 1024, true);
            using var jtw = new JsonTextWriter(sw) { Formatting = Formatting.None };
            var js = new JsonSerializer();
            js.Serialize(jtw, value);
            jtw.Flush();
        }
    }
}
