using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OnBoardClientel.Functions.Services;
using SendGrid.Helpers.Mail;

namespace OnBoardClientel.Functions.Functions
{
    public class RegistrationDurableFunction
    {
        private readonly OnBoardClientelContext dbContext;
        private readonly HttpClient httpClient;

        public RegistrationDurableFunction(OnBoardClientelContext onBoardClientelContext, IHttpClientFactory httpClientFactory)
        {
            dbContext = onBoardClientelContext;
            httpClient = httpClientFactory.CreateClient();
        }


        [FunctionName("NewClient")]
        public async Task<HttpResponseMessage> StartRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter, ILogger log)
        {
            try
            {
                //deserialize to Client object
                var content = await req.Content.ReadAsStringAsync();
                var client = JsonConvert.DeserializeObject<Client>(content);

               
                string instanceId = await starter.StartNewAsync("ProcessNewClient", client);
                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                return starter.CreateCheckStatusResponse(req, instanceId);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error Registering client");
                return req.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "Error Registering client");
            }
        }


        [FunctionName("ProcessNewClient")]
        public async Task NewClientOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var client = context.GetInput<Client>();
            client.DurableFunctionUrl = context.InstanceId;

            //save client
            client = await context.CallActivityAsync<Client>("SaveClient", client);

            //generate documents
            client = await context.CallActivityAsync<Client>("GenerateClientDocument", client);

            //send Email
            await context.CallActivityAsync("SendClientEmail", client);
        }


        [FunctionName("SaveClient")]
        public async Task<Client> SaveClientToDatabase([ActivityTrigger] Client client)
        {
            client.CreatedTime = DateTime.Now;

            dbContext.Clients.Add(client);
            await dbContext.SaveChangesAsync();
            return client;
        }


        [FunctionName("GenerateClientDocument")]
        public async Task<Client> GenerateClientDocument([ActivityTrigger] Client client, ILogger log,
            [Blob("clients1/{rand-guid}.txt", FileAccess.ReadWrite, Connection = "AzureStorageConnectionString")] CloudBlockBlob blob)
        {

            var response = await httpClient.GetAsync(client.Url);
            var html = await response.Content.ReadAsStringAsync();
            var bytes = Encoding.ASCII.GetBytes(html);

            //long running process
            await Task.Delay(1000 * new Random().Next(10, 30));
            await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);

            client.DocumentUrl = blob.Uri.ToString();
            client.DocumentGenerated = DateTime.Now;

            dbContext.Clients.Update(client);
            await dbContext.SaveChangesAsync();

            return client;
        }


        [FunctionName("SendClientEmail")]
        [return: SendGrid(To = "golois.mouelet@microsoft.com", From = "golois.mouelet@microsoft.com")] //TODO config
        public async Task<SendGridMessage> SendClientEmail([ActivityTrigger] Client client, ILogger log)
        {
            var message = new SendGridMessage();
            message.AddContent("text/html", EmailContent(client));
            message.Subject = $"{client.Name} - Onboarding Confirmation";

            var response = await httpClient.GetAsync(client.DocumentUrl);
            var bytes = await response.Content.ReadAsByteArrayAsync();

            message.AddAttachment(new Attachment()
            {
                Content = Convert.ToBase64String(bytes),
                Filename = $"{client.Name}-Document.txt",
                Type = "text/plain",
                Disposition = "attachment"
            });

            client.EmailSent = DateTime.Now;
            dbContext.Clients.Update(client);
            await dbContext.SaveChangesAsync();

            return message;
        }


        private string EmailContent(Client client)
        {
            return "<h2>OnBoardingClientel - Welcome & Confirmation email </h2>" +
                "<strong>Date:</strong>  " + DateTime.Now.ToString("MMMM dd, yyyy") + "<br />" +
                "<strong>Name:</strong>  " + client.Name + "<br />" +
                "<strong>Industry:</strong>  " + client.Industry + "<br />" +
                "<strong>Website:</strong>  " + client.Url + "<br />" +
                "<p>Thank you for choosing OnBoardingClient!" +
                " This a confirmation email affirming that you have been successfully onboarded" +
                " on our scalable system where you will have access to monitor and review your compliance." +
                " The following are your next action item to complete your compliance.</p>" +
                "<h3>Action Items</h3>" +
                "<ul>" +
                "<li> Review your name, industry, and url information listed aboved. </li> " +
                "<li> Download and review the attached document generated </li>" +
                "<li> Login to the compliance board and review your current standing </li>" +
                "</ul> " +
                "<p> If you have any questions, please feel free to contact our call x1234 and our customer representatives" +
                " will be able to help. <br />" +
                "Sincerely, <br/ > <br/>" +
                "Your Onboarding & Compliance Advisor <br/>" +
                "OnboardingClient.com </p>";
        }
    }
}