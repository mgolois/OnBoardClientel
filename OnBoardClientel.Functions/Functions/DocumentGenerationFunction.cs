using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OnBoardClientel.Functions.Services;

namespace OnBoardClientel.Functions.Functions
{
    public class DocumentGenerationFunction
    {
        private readonly HttpClient httpClient;
        private readonly OnBoardClientelContext dbContext;
        public DocumentGenerationFunction(IHttpClientFactory httpClientFactory, OnBoardClientelContext onBoardClientelContext)
        {
            httpClient = httpClientFactory.CreateClient();
            dbContext = onBoardClientelContext;
        }

        [FunctionName("DocumentGeneration")]
        public async Task GenerateDocument(
            [ServiceBusTrigger("%ServiceBusQueueName%", Connection = "ServiceBusConnectionString")]string myQueueItem,
            ILogger log,
            [Blob("clients/{rand-guid}.txt", FileAccess.ReadWrite, Connection = "AzureStorageConnectionString")] CloudBlockBlob blob)
        {
            var client = JsonConvert.DeserializeObject<Client>(myQueueItem);
            var html = await FetchHtml(client.Url);
            var bytes = Encoding.ASCII.GetBytes(html);

            //long running process
            await Task.Delay(1000 * new Random().Next(10, 30));
            
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);

            client.DocumentUrl = blob.Uri.ToString();
            client.DocumentGenerated = DateTime.Now;
            dbContext.Clients.Update(client);
            await dbContext.SaveChangesAsync();

        }

        private async Task<string> FetchHtml(string url)
        {
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"FetchHtml failed {response.StatusCode} : {response.ReasonPhrase}");
            }

            return await response.Content.ReadAsStringAsync();
        }

    }
}
