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
using Microsoft.Azure.WebJobs.ServiceBus;

namespace OnBoardClientel.Functions.Functions
{
    public class ClientFunction
    {
        private OnBoardClientelContext dbContext;
        public ClientFunction(OnBoardClientelContext onBoardClientelContext)
        {
            dbContext = onBoardClientelContext;
        }

        [FunctionName("GetClients")]
        public async Task<IActionResult> Get([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,ILogger log)
        {
            try
            {
                var clients = await dbContext.Clients.ToListAsync();

                return new OkObjectResult(clients);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Error Getting clients");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
