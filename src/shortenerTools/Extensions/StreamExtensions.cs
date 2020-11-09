using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace shortenerTools.Extensions
{
    public static class StreamExtensions
    {
        public static T DeserializeJsonFromStream<T>(this Stream stream)
        {
            if (stream == null || !stream.CanRead)
                return default;

            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);
            var js = new JsonSerializer();
            var searchResult = js.Deserialize<T>(jtr);
            return searchResult;
        }

        public static async Task<string> StreamToStringAsync(this Stream stream)
        {
            if (stream == null)
                return null;

            using var sr = new StreamReader(stream);
            var content = await sr.ReadToEndAsync();

            return content;
        }
    }
}