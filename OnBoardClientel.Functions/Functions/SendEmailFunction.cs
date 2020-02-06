using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using SendGrid.Helpers.Mail;
using Microsoft.Azure.WebJobs.Extensions.Http;
using OnBoardClientel.Functions.Services;
using System.Linq;
using System.Threading.Tasks;

namespace OnBoardClientel.Functions.Functions
{

    public class SendEmailFunction
    {
        private readonly OnBoardClientelContext dbContext;

        public SendEmailFunction(OnBoardClientelContext onBoardClientelContext)
        {
            dbContext = onBoardClientelContext;
        }
        [FunctionName("SendEmail")]
        public void SendConfirmationEmail([BlobTrigger("clients/{name}", Connection = "AzureStorageConnectionString")]Stream stream,
            string name, ILogger log,
            [SendGrid(To = "email@address.com", From = "email@address.com")] out SendGridMessage message)
        {

            var client = dbContext.Clients.FirstOrDefault(c => c.DocumentUrl.Contains(name));

            message = new SendGridMessage();
            message.AddContent("text/html", EmailContent(client));
            message.Subject = $"{client.Name} - Onboarding Confirmation";

            message.AddAttachment(new Attachment()
            {
                Content = StreamToBase64String(stream),
                Filename = $"{client.Name}-Document.txt",
                Type = "text/plain",
                Disposition = "attachment"
            });

            client.EmailSent = DateTime.Now;
            dbContext.SaveChanges();
        }

        private string StreamToBase64String(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
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
