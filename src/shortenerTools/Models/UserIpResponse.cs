using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace shortenerTools.Models
{
    [ExcludeFromCodeCoverage]
    public class UserIpResponse
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        [JsonProperty("country_name")]
        public string CountryName { get; set; }
        [JsonProperty("region_code")]
        public string RegionCode { get; set; }
        [JsonProperty("region_name")]
        public string RegionName { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("zip_code")]
        public string ZipCode { get; set; }
        [JsonProperty("time_zone")]
        public string TimeZone { get; set; }
        [JsonProperty("latitude")]
        public float Latitude { get; set; }
        [JsonProperty("longitude")]
        public float Longitude { get; set; }
        [JsonProperty("metro_code")]
        public int MetroCode { get; set; }
    }
}