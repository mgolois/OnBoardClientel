using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnBoardClientel.Functions.Services
{
    public interface IQueueSender
    {
        Task SendMessage<T>(T obj);
        Task SendMessages(List<string> messages);
    }
    public class ServiceBusQueueSender : IQueueSender
    {
        private IQueueClient queueClient;
        public ServiceBusQueueSender()
        {
            var queueName = Environment.GetEnvironmentVariable("ServiceBusQueueName");
            var connStr = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
            queueClient = new QueueClient(connStr, queueName);
        }
        public async Task SendMessage<T>(T obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            var msgBytes = Encoding.UTF8.GetBytes(json);
            await queueClient.SendAsync(new Message(msgBytes)); 
        }

        public async Task SendMessages(List<string> messages)
        {
            var queueMessages = messages.Select(m => new Message(Encoding.UTF8.GetBytes(m))).ToList();
            await queueClient.SendAsync(queueMessages);
        }
    }
}
