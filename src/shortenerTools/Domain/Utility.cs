using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Cloud5mins.domain
{
    public static class Utility
    {
        //reshuffled for randomisation, same unique characters just jumbled up, you can replace with your own version
        private const string ConversionCode = "FjTG0s5dgWkbLf_8etOZqMzNhmp7u6lUJoXIDiQB9-wRxCKyrPcv4En3Y21aASHV";
        private static readonly int Base = ConversionCode.Length;
        //sets the length of the unique code to add to vanity
        private const int MinVanityCodeLength = 5;

        public static async Task<string> GetValidEndUrl(string vanity, StorageTableHelper stgHelper)
        {
            if (string.IsNullOrEmpty(vanity))
            {
                var newKey = await stgHelper.GetNextTableId();
                string getCode() => Encode(newKey);
                if (await stgHelper.IfShortUrlEntityExistByVanity(getCode()))
                    return await GetValidEndUrl(vanity, stgHelper);

                return string.Join(string.Empty, getCode());
            }
            else
            {
                return string.Join(string.Empty, vanity);
            }
        }

        public static string Encode(int i)
        {
            if (i == 0)
                return ConversionCode[0].ToString();

            return GenerateUniqueRandomToken(i);
        }

        public static string GetShortUrl(string host, string vanity){
               return host + "/" + vanity;
        }

        // generates a unique, random, and alphanumeric token for the use as a url 
        //(not entirely secure but not sequential so generally not guessable)
        public static string GenerateUniqueRandomToken(int uniqueId)
        {
            //Encode the unique id prefix (less characters and prevent displaying url count)
            var s = string.Empty;
            while (uniqueId > 0)
            {
                s += ConversionCode[uniqueId % Base];
                uniqueId = uniqueId / Base;
            }

            using (var generator = new RNGCryptoServiceProvider())
            {
                //minimum size I would suggest is 5, longer the better but we want short URLs!
                var bytes = new byte[MinVanityCodeLength];
                generator.GetBytes(bytes);
                var chars = bytes
                    .Select(b => ConversionCode[b % Base]);
                var token = new string(chars.ToArray());
                return string.Join(string.Empty, s.Reverse().Concat(token.Reverse()));
            }
        }
    }
}