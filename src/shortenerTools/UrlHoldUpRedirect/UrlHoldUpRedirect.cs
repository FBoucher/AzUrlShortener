using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Cloud5mins.Function
{
    public static class UrlHoldUpRedirect
    {
        [FunctionName("UrlHoldUpRedirect")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UrlHoldUpRedirect/{shortUrl}")] HttpRequestMessage req,
            string shortUrl, 
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed for Url: {shortUrl}");

            string redirectUrl = "https://azure.com";

            if (!String.IsNullOrWhiteSpace(shortUrl))
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                redirectUrl = config["defaultRedirectUrl"];

                StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]); 

                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);
                
                var newUrl = await stgHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    log.LogInformation($"Found it: {newUrl.Url}");
                    newUrl.Clicks++;
                    stgHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await stgHelper.SaveShortUrlEntity(newUrl);
                    redirectUrl = newUrl.Url
                    ;
                }
            }
            else
            {
                log.LogInformation("Bad Link, resorting to fallback.");
            }

            var fileName = "D:\\home\\site\\repository\\medias\\UrlHoldUpRedirect.html";
            var fileContent = "";

            try
            {
                var fileInfo = new FileInfo(fileName);
        
                // Prüfen ob die Datei existiert
                if (fileInfo.Exists)
                {
                    // Datei in einen FileStream laden
                    var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read);
        
                    // StreamReader initialisieren
                    var reader = new StreamReader(fileStream);
        
                    String line;
        
                    // Lese Datei, Zeile für Zeile
                    while ((line = reader.ReadLine()) != null)
                    {
                        fileContent += line + "\n";
                    }
        
                    reader.Close();
                    fileStream.Close();
                }
                else
                {
                        fileContent = "File "+fileName+" not found!";
                }
            }
            catch (IOException ex)
            {
                fileContent = "File "+fileName+" cant be accessed!";
                log.LogInformation("File "+fileName+" cant be accessed!"+ex.Message);
            }

            //Template Replacements
            fileContent = fileContent.Replace("%%redirectUrl%%",redirectUrl);
            fileContent = fileContent.Replace("%%HtmlEncoded_redirectUrl%%",WebUtility.HtmlEncode(redirectUrl));

            //Return Response
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(fileContent);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;

        }
  }
}
