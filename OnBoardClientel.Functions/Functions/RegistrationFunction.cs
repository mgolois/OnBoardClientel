using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OnBoardClientel.Functions.Services;
using Microsoft.EntityFrameworkCore;

namespace OnBoardClientel.Functions.Functions
{
    public class RegistrationFunction
    {
        private OnBoardClientelContext dbContext;
        private IQueueSender queueSender;
        public RegistrationFunction(OnBoardClientelContext onBoardClientelContext, IQueueSender sender)
        {
            dbContext = onBoardClientelContext;
            queueSender = sender;
        }


        /// <summary>
        /// Saves new client in database and sends newly created client to queue
        /// </summary>
        [FunctionName("RegisterClient")]
        public async Task<IActionResult> Post([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                                                            ILogger log)
        {
            try
            {
                //deserialize to Client object
                var content = await req.ReadAsStringAsync();
                var client = JsonConvert.DeserializeObject<Client>(content);

                client.CreatedTime = DateTime.Now;

                dbContext.Clients.Add(client);
                await dbContext.SaveChangesAsync();

                //send message to queue
                await queueSender.SendMessage(client);

                return new OkObjectResult(client);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error Registering client");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
