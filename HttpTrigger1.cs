using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace HttpTrigger1
{
    public static class HttpTrigger1
    {
        private static readonly HttpClient client = new HttpClient();

        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string personalAccessToken = req.Query["personalAccessToken"];
            string organization = req.Query["organization"];
            string feedId = req.Query["feedId"];
            string project = req.Query["project"];
            string packageId = req.Query["packageId"];
            string version = req.Query["version"];
            string filename = req.Query["filename"];

            using (HttpClient client = new HttpClient())
            {
                //set our headers
                //pass in the access token to authenticate the request
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", personalAccessToken))));
                client.DefaultRequestHeaders.Add("Accept", "application/json;api-version=7.0-preview;excludeUrls=true");

                //set url to packages endpoint
                //format may change with different package types
                var url = $"https://pkgs.dev.azure.com/{organization}/{project}/_packaging/{feedId}/nuget/v3/flat2/{packageId}/{version}/{filename}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    log.LogInformation("Success");
                    return new FileStreamResult(content, "application/octet-stream");                }
                else
                {
                    log.LogInformation("Failure");
                    log.LogInformation("Response: " + response);
                    log.LogInformation(url);
                    return new BadRequestObjectResult("Failed to download package");
                }
            }            
        }
    }
}

